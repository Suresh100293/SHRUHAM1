using ALT_ERP3.Areas.Accounts.Models;
using ALT_ERP3.Areas.Logistics.Models;
using ALT_ERP3.Areas.Vehicles.Models;
using ALT_ERP3.Controllers;
using Common;
using CrystalDecisions.CrystalReports.Engine;
using EntitiModel;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.Reporting.WebForms;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ALT_ERP3.Areas.Vehicles.Controllers
{
    public class TripSheetController : BaseController
    {
        public new string StartDate = System.Web.HttpContext.Current.Session["StartDate"].ToString();
        public new string EndDate = System.Web.HttpContext.Current.Session["LastDate"].ToString();

        private static string mauthorise = "A00";

        public ActionResult CheckDriverTripDates(TripSheetVM mModel)//Check Duplicate Date Of Tripsheet
        {
            bool DuplicateDateFound = false;
            var TripToDate = ConvertDDMMYYTOYYMMDD(mModel.TODate);
            var TripFromDate = ConvertDDMMYYTOYYMMDD(mModel.FromDate);
            var Datarange = Enumerable.Range(0, (TripToDate - TripFromDate).Days + 1)
                          .Select(offset => TripFromDate.AddDays(offset))
                          .ToList();

            if (mModel.VehicleFlag)
            {
                // Step 1: Fetch records matching the driver condition from the database
                var preliminaryResults = ctxTFAT.TripSheetMaster
                    .Where(tsm => tsm.Driver == mModel.VehicleCode)
                    .ToList();

                // Step 2: Perform date range filtering in memory
                var TripSheetMaster = preliminaryResults
                    .Where(tsm => tsm.FromDT.HasValue && tsm.TODT.HasValue &&
                                  Datarange.Any(date => date >= tsm.FromDT.Value.AddDays(1) && date <= tsm.TODT.Value.AddDays(-1)))
                    .ToList();


                //var TripSheetMaster = ctxTFAT.TripSheetMaster.Where(x => x.Driver == mModel.VehicleCode &&( x.FromDT > TripFromDate || x.TODT < TripToDate)).ToList();
                if (TripSheetMaster.Count() > 0)
                {
                    DuplicateDateFound = true;
                }
            }
            else
            {
                // Step 1: Fetch records matching the driver condition from the database
                var preliminaryResults = ctxTFAT.TripSheetMaster
                    .Where(tsm => tsm.Driver == mModel.DriverCode)
                    .ToList();

                // Step 2: Perform date range filtering in memory
                var TripSheetMaster = preliminaryResults
                    .Where(tsm => tsm.FromDT.HasValue && tsm.TODT.HasValue &&
                                  Datarange.Any(date => date >= tsm.FromDT.Value.AddDays(1) && date <= tsm.TODT.Value.AddDays(-1)))
                    .ToList();

                //var TripSheetMaster = ctxTFAT.TripSheetMaster.Where(x => x.Driver == mModel.DriverCode && (x.FromDT > TripFromDate || x.TODT < TripToDate)).ToList();
                if (TripSheetMaster.Count() > 0)
                {
                    DuplicateDateFound = true;
                }
            }

            return Json(new
            {
                DuplicateDateFound = DuplicateDateFound,
            }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult GetTDSList(string term)
        {
            if (term == "" || term == null)
            {
                var result = ctxTFAT.TDSMaster.Select(c => new { c.Code, c.Name }).Distinct().ToList();
                return Json(result, JsonRequestBehavior.AllowGet);

            }
            else
            {
                var result = ctxTFAT.TDSMaster.Where(x => x.Name.Contains(term)).Select(m => new { m.Code, m.Name }).Distinct().ToList();
                return Json(result, JsonRequestBehavior.AllowGet);

            }
        }

        public ActionResult GetTDSRateDetail(TripSheetVM Model)
        {
            var CurrDate = ConvertDDMMYYTOYYMMDD(Model.TripSheetDate);
            var TDSRate = ctxTFAT.TDSRates.Where(x => x.Code.ToString() == Model.TDSCode && x.EffDate <= CurrDate).OrderByDescending(x => x.EffDate).Select(x => new { x.TDSRate/*, x.Cess, x.SurCharge, x.SHECess*/ }).FirstOrDefault();
            if (TDSRate != null)
            {
                Model.TDSRate = (TDSRate.TDSRate == null) ? 0 : Convert.ToDecimal(TDSRate.TDSRate.Value);
            }
            else
            {
                Model.TDSRate = 0;
            }
            return Json(Model, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetDriverList(string term)
        {
            var list = ctxTFAT.DriverMaster.Where(x => x.Status == true).ToList();

            if (!String.IsNullOrEmpty(term))
            {
                list = ctxTFAT.DriverMaster.Where(x => x.Status == true && x.Name.Contains(term)).ToList();
            }

            var Modified = list.Select(x => new
            {
                Code = x.Code,
                Name = x.Name
            });
            return Json(Modified, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetVehicle(string term)
        {
            var list = ctxTFAT.VehicleMaster.Where(x => x.Acitve == true).Select(x => new { x.TruckNo, x.Code, x.TruckStatus }).ToList();

            if (!(String.IsNullOrEmpty(term)))
            {
                list = list.Where(x => x.TruckNo.Contains(term.ToLower())).ToList();
            }
            var Modified = list.Select(x => new
            {
                Code = x.Code,
                Name = x.TruckNo + (x.TruckStatus == "100001" ? " -H " : x.TruckStatus == "100000" ? " -A" : " -O")
            });
            return Json(Modified, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetTripDebitAcList(string term)
        {
            var list = ctxTFAT.Master.Where(x => x.AcType == "X").ToList();

            if (!String.IsNullOrEmpty(term))
            {
                list = list.Where(x => x.AcType == "X" && x.Name.ToLower().Contains(term.ToLower())).ToList();
            }
            var Modified = list.Select(x => new
            {
                Code = x.Code,
                Name = x.Name
            });
            return Json(Modified, JsonRequestBehavior.AllowGet);
        }


        // GET: Vehicles/TripSheet
        public ActionResult Index(TripSheetVM mModel)
        {

            TempData.Remove("PendingFM");
            TempData.Remove("SelecedFM");
            Session["OtherExpensesList"] = null;
            Session["OtherDeductionList"] = null;
            GetAllMenu(Session["ModuleName"].ToString());
            UpdateAuditTrail(mbranchcode, mModel.Mode, mModel.Header, "", DateTime.Now, 0, "", "", "NA");

            List<GridOption> Grlist = new List<GridOption>();
            var list = ctxTFAT.DocFormats.Where(x => x.Type == "Trip0").Select(x => x).ToList();
            if (list != null)
            {
                foreach (var a in list)
                {
                    Grlist.Add(new GridOption
                    {
                        Format = a.FormatCode,
                        IsFormatSelected = a.Selected,
                        StoreProcedure = a.StoredProc
                    });
                }

            }
            mModel.PrintGridList = Grlist;

            #region SetUp

            var TripSetup = ctxTFAT.TripSheetSetup.FirstOrDefault();
            if (TripSetup == null)
            {
                TripSetup = new TripSheetSetup();
            }
            if (!String.IsNullOrEmpty(TripSetup.DebitAc))
            {
                mModel.TripDebitAc = TripSetup.DebitAc;
                mModel.TripDebitAcName = ctxTFAT.Master.Where(x => x.Code == TripSetup.DebitAc).Select(x => x.Name).FirstOrDefault();
            }
            mModel.ChangeAC = TripSetup.ChgDebitAc;
            mModel.TDSDeduction = TripSetup.TDSDeduction;
            mModel.ChangeCharge = TripSetup.ChangeChrgAmt;
            //mModel.CostCenter = TripSetup.CostCenter;
            mModel.NoDocumentReq = TripSetup.NoDocumentAllow;
            mModel.ConfirmDupDateOfTrip = TripSetup.ConfirmDupDateOfTrip;
            mModel.RestrictDupDateOfTrip = TripSetup.RestrictDupDateOfTrip;
            mModel.Pick_Financial_Document = TripSetup.Pick_Financial_Document;
            if (TripSetup.CurrDatetOnlyreq == false && TripSetup.BackDateAllow == false && TripSetup.ForwardDateAllow == false)
            {
                mModel.StartDate = ConvertDDMMYYTOYYMMDD(StartDate).ToString("yyyy-MM-dd");
                mModel.EndDate = ConvertDDMMYYTOYYMMDD(EndDate).ToString("yyyy-MM-dd");
            }
            if (TripSetup.CurrDatetOnlyreq == true)
            {
                mModel.StartDate = DateTime.Now.ToString("yyyy-MM-dd");
                mModel.EndDate = DateTime.Now.ToString("yyyy-MM-dd");
            }
            if (TripSetup.BackDateAllow == true)
            {
                mModel.StartDate = (DateTime.Now.AddDays(-TripSetup.BackDaysUpto)).ToString("yyyy-MM-dd");
            }
            if (TripSetup.ForwardDateAllow == true)
            {
                mModel.EndDate = (DateTime.Now.AddDays(TripSetup.ForwardDaysUpto)).ToString("yyyy-MM-dd");
            }

            #endregion


            if (String.IsNullOrEmpty(mModel.Prefix))
            {
                mModel.Prefix = mperiod;
            }


            if ((mModel.Mode == "Edit") || (mModel.Mode == "View") || (mModel.Mode == "Delete"))
            {
                var mList = ctxTFAT.TripSheetMaster.Where(x => x.DocNo.Trim() == mModel.Document.Trim() && x.Prefix == mModel.Prefix).FirstOrDefault();
                if (mList != null)
                {
                    #region Modal Bind
                    mModel.TripDebitAc = mList.DebitAc;
                    mModel.TripDebitAcName = ctxTFAT.Master.Where(x => x.Code == mList.DebitAc).Select(x => x.Name).FirstOrDefault();
                    mModel.TripSheetNo = mList.DocNo;
                    mModel.TripSheetDate = mList.DocDate.ToShortDateString();
                    mModel.DocDate = mList.DocDate;
                    mModel.VehicleFlag = mList.VehicleFlag;
                    if (mList.VehicleFlag)
                    {
                        mModel.VehicleCode = mList.Driver;
                        mModel.VehicleName = ctxTFAT.VehicleMaster.Where(x => x.Code == mList.Driver).Select(x => x.TruckNo).FirstOrDefault();
                    }
                    else
                    {
                        mModel.DriverCode = mList.Driver;
                        mModel.DriveName = ctxTFAT.DriverMaster.Where(x => x.Code == mList.Driver).Select(x => x.Name).FirstOrDefault();
                    }
                    mModel.Narr = mList.Narr;
                    mModel.NetAmt = mList.NetAmt;
                    mModel.TDSAmt = mList.TDSAmt;

                    mModel.FromDate = mList.FromDT.Value.ToShortDateString();
                    mModel.TODate = mList.TODT.Value.ToShortDateString();


                    mModel.AdvFromDate = mList.AdvFromDT == null ? DateTime.Now.ToShortDateString() : mList.AdvFromDT.Value.ToShortDateString();
                    mModel.AdvTODate = mList.AdvTODT == null ? DateTime.Now.ToShortDateString() : mList.AdvTODT.Value.ToShortDateString();
                    mModel.CutAdv = mList.CutAdv;

                    mModel.BalFromDate = mList.BalFromDT == null ? DateTime.Now.ToShortDateString() : mList.BalFromDT.Value.ToShortDateString();
                    mModel.BalTODate = mList.BalTODT == null ? DateTime.Now.ToShortDateString() : mList.BalTODT.Value.ToShortDateString();


                    mModel.CCFromDate = mList.CCFromDT == null ? DateTime.Now.ToShortDateString() : mList.CCFromDT.Value.ToShortDateString();
                    mModel.CCTODate = mList.CCTODT == null ? DateTime.Now.ToShortDateString() : mList.CCTODT.Value.ToShortDateString();
                    mModel.CutCC = mList.CutCC;

                    mModel.FromKM = mList.FromKM == null ? "0" : mList.FromKM;
                    mModel.ToKM = mList.ToKM == null ? "0" : mList.ToKM;
                    mModel.RunningKM = (Convert.ToInt32(mList.ToKM) - Convert.ToInt32(mList.FromKM)).ToString();
                    mModel.PerKMChrg = mList.PerKMChrg.Value;

                    mModel.TripChrgKMExp = Convert.ToDecimal(Convert.ToDecimal(mModel.PerKMChrg) * Convert.ToInt32(mModel.RunningKM));
                    mModel.TripChrgKMExp = Math.Round(mModel.TripChrgKMExp, 2);

                    var Branch = mList.ParentKey.Substring(0, 6);
                    if (ctxTFAT.TfatBranch.Where(x => x.Code == Branch).FirstOrDefault() != null)
                    {
                        if (mList.ParentKey.Substring(6, 3) != "Tri")
                        {
                            mModel.RefDocument = true;
                        }
                    }
                    else
                    {
                        if (mList.ParentKey.Substring(0, 3) != "Tri")
                        {
                            mModel.RefDocument = true;
                        }
                    }
                    #endregion

                    #region Posting Account
                    var PostAc = "";
                    if (mList.VehicleFlag)
                    {
                        PostAc = ctxTFAT.VehicleMaster.Where(x => x.Code == mList.Driver).Select(x => x.PostAc).FirstOrDefault();
                    }
                    else
                    {
                        PostAc = ctxTFAT.DriverMaster.Where(x => x.Code == mList.Driver).Select(x => x.Posting).FirstOrDefault();
                    }
                    #endregion

                    #region Advance Ledger List

                    var AdvSelected = mList.AdjustLedgerRef.Split('^').ToList();
                    var LedgerList = ctxTFAT.Ledger.Where(x => AdvSelected.Any(y => y == x.Branch + x.TableKey) && x.Code == PostAc && x.Debit != 0 && x.Type.ToLower() != "trip0").ToList();
                    //List<Ledger> ledgers = new List<Ledger>();
                    var ledgers = LedgerList.Select(x => new Ledger
                    {
                        DocDate = x.DocDate,
                        Debit = x.Debit,
                        Credit = x.Credit,
                        Code = ctxTFAT.Master.Where(y => y.Code == x.Code).Select(y => y.Name).FirstOrDefault(),
                        AltCode = ctxTFAT.Master.Where(y => y.Code == x.AltCode).Select(y => y.Name).FirstOrDefault(),
                        TableKey = x.TableKey,
                        Branch = x.Branch,
                        Narr = x.Narr,
                        Srl = x.Srl,
                        Party = ctxTFAT.TfatBranch.Where(y => y.Code == x.Branch).Select(y => y.Name).FirstOrDefault(),
                    }).ToList();

                    mModel.Advledgers = ledgers;
                    TempData["LedgerList"] = ledgers;
                    #endregion

                    #region Balance Ledger List

                    var BalSelected = mList.AdjustBalLedgerRef == null ? "".Split('^') : mList.AdjustBalLedgerRef.Split('^');
                    LedgerList = ctxTFAT.Ledger.Where(x => BalSelected.Any(y => y == x.Branch + x.TableKey) && x.Code == PostAc && x.Credit != 0 && x.Type.ToLower() != "trip0").ToList();
                    //List<Ledger> Balledgers = new List<Ledger>();
                    var Balledgers = LedgerList.Select(x => new Ledger
                    {
                        DocDate = x.DocDate,
                        Debit = x.Debit,
                        Credit = x.Credit,
                        Code = ctxTFAT.Master.Where(y => y.Code == x.Code).Select(y => y.Name).FirstOrDefault(),
                        AltCode = ctxTFAT.Master.Where(y => y.Code == x.AltCode).Select(y => y.Name).FirstOrDefault(),
                        TableKey = x.TableKey,
                        Branch = x.Branch,
                        Narr = x.Narr,
                        Srl = x.Srl,
                        Party = ctxTFAT.TfatBranch.Where(y => y.Code == x.Branch).Select(y => y.Name).FirstOrDefault(),
                    }).ToList();
                    mModel.Balledgers = Balledgers;
                    TempData["BalLedgerList"] = Balledgers;
                    #endregion

                    #region Cost Center Ledger List
                    var ListLEgerRef = ctxTFAT.RelateData.Where(x => x.AmtType == true && PostAc.ToLower().Trim() == x.Value8.ToLower().Trim()).Select(x => x.ParentKey.Trim() + x.Branch.Trim() + x.Code.Trim()).ToList();
                    var CCLedgerList = ctxTFAT.Ledger.Where(x => ListLEgerRef.Contains(x.ParentKey.Trim() + x.Branch.Trim() + x.Code.Trim()) && x.Debit != 0 && x.DocDate >= mList.CCFromDT && x.DocDate <= mList.CCTODT && x.Type.ToLower() != "trip0").ToList();
                    List<Ledger> CCledgers = new List<Ledger>();
                    foreach (var item in CCLedgerList)
                    {
                        var RelaTeData = ctxTFAT.RelateData.Where(x => x.AmtType == true && PostAc.ToLower().Trim() == x.Value8.ToLower().Trim() && x.ParentKey + x.Branch == item.ParentKey + item.Branch && (x.Combo1 == "000100343" || x.Code == "000100343")).FirstOrDefault();
                        //var Check = ctxTFAT.TripSheetMaster.Where(x => x.DocNo != mModel.Document && x.CCjustLedgerRef.Contains(item.Branch + item.TableKey) && x.Prefix == mList.Prefix).FirstOrDefault();
                        var allkey = ctxTFAT.TripSheetMaster.Where(x => x.DocNo != mModel.Document).Select(x => x.CCjustLedgerRef).ToList();
                        List<string> List = new List<string>();
                        foreach (var key in allkey)
                        {
                            List.AddRange(key.Split('^'));
                        }
                        var Check = List.Where(x => x == item.Branch + item.TableKey).FirstOrDefault();
                        if (Check == null)
                        {
                            CCledgers.Add(new Ledger
                            {
                                DocDate = item.DocDate,
                                Debit = item.Debit,
                                Credit = item.Credit,
                                Code = ctxTFAT.Master.Where(x => x.Code == item.Code).Select(x => x.Name).FirstOrDefault(),
                                AltCode = ctxTFAT.Master.Where(x => x.Code == item.AltCode).Select(x => x.Name).FirstOrDefault(),
                                Narr = item.Narr,
                                TableKey = item.TableKey,
                                Branch = item.Branch,
                                RefDoc = RelaTeData == null ? "0" : (String.IsNullOrEmpty(RelaTeData.Value2) == true ? "0" : RelaTeData.Value2),
                                AUTHORISE = RelaTeData == null ? "" : (String.IsNullOrEmpty(RelaTeData.Value4) == true ? "" : RelaTeData.Value4),
                                AUTHIDS = RelaTeData == null ? "0" : (String.IsNullOrEmpty(RelaTeData.Value3) == true ? "0" : RelaTeData.Value3),
                                Party = ctxTFAT.TfatBranch.Where(x => x.Code == item.Branch).Select(x => x.Name).FirstOrDefault(),
                            });
                        }
                    }
                    if (!(String.IsNullOrEmpty(mList.CCjustLedgerRef)))
                    {
                        var CCLedger = mList.CCjustLedgerRef.Split('^');
                        var CCCutLedger = new List<string>().ToList();
                        if (!(String.IsNullOrEmpty(mList.CCLedgerRefCutFromTrip)))
                        {
                            CCCutLedger = mList.CCLedgerRefCutFromTrip.Split('^').ToList();
                        }
                        foreach (var item in CCledgers)
                        {
                            if (CCLedger.Contains(item.Branch + item.TableKey))
                            {
                                item.TDSFlag = true;
                            }
                            if (CCCutLedger.Contains(item.Branch + item.TableKey))
                            {
                                item.Reminder = true;
                            }
                        }
                    }
                    mModel.CCledgers = CCledgers;
                    #endregion

                    #region Document List
                    mModel.FMOrNOt = mList.FMorNOT;
                    if (mModel.FMOrNOt)
                    {
                        var FMList = (from FreightMemo in ctxTFAT.TripFmList
                                      where FreightMemo.DocNo == mModel.Document && FreightMemo.Prefix == mList.Prefix
                                      orderby FreightMemo.FMNo
                                      select new FMMasterTrip()
                                      {
                                          RefTablekey = FreightMemo.RefTablekey,
                                          FMNo = FreightMemo.FMNo.ToString(),
                                          Date = FreightMemo.FmDate,
                                          VehicleNo = String.IsNullOrEmpty(ctxTFAT.VehicleMaster.Where(x => x.Code == FreightMemo.VehicleNo).Select(x => x.TruckNo).FirstOrDefault()) == true ? ctxTFAT.HireVehicleMaster.Where(x => x.Code == FreightMemo.VehicleNo).Select(x => x.TruckNo).FirstOrDefault() : ctxTFAT.VehicleMaster.Where(x => x.Code == FreightMemo.VehicleNo).Select(x => x.TruckNo).FirstOrDefault(),
                                          // Driver = ctxTFAT.FMMaster.Where(x => x.TableKey == FreightMemo.RefTablekey).Select(x => x.Name).FirstOrDefault(),
                                          DriverC = ctxTFAT.FMMaster.Where(x => x.TableKey == FreightMemo.RefTablekey).Select(x => x.Driver).FirstOrDefault(),
                                          From = ctxTFAT.TfatBranch.Where(x => x.Code == FreightMemo.FromBranch).Select(x => x.Name).FirstOrDefault(),
                                          FromC = FreightMemo.FromBranch,
                                          To = ctxTFAT.TfatBranch.Where(x => x.Code == FreightMemo.ToBranch).Select(x => x.Name).FirstOrDefault(),
                                          ToC = FreightMemo.ToBranch,
                                          RouteVia = ctxTFAT.FMMaster.Where(x => x.TableKey.ToString() == FreightMemo.RefTablekey).Select(x => x.RouteViaName).FirstOrDefault(),
                                          RouteViaC = FreightMemo.RouteVia,
                                          Tripchages = FreightMemo.TripChrg,
                                          LocalCharges = FreightMemo.LocalChrg,
                                          ViaCharges = FreightMemo.ViaChrg,
                                          Total = (FreightMemo.TripChrg + FreightMemo.LocalChrg + FreightMemo.ViaChrg),
                                          PaymentAmt = ctxTFAT.FMMaster.Where(x => x.TableKey.ToString() == FreightMemo.RefTablekey).Select(x => x.Adv).FirstOrDefault(),
                                          DieselLtr = ctxTFAT.FMMaster.Where(x => x.TableKey.ToString() == FreightMemo.RefTablekey).Select(x => x.DieselLtr ?? "0").FirstOrDefault(),
                                      }).ToList();
                        foreach (var GetDriverName in FMList)
                        {
                            GetDriverName.Driver = ctxTFAT.DriverMaster.Where(x => x.Code == GetDriverName.DriverC).Select(x => x.Name).FirstOrDefault();
                        }
                        mModel.fMMasters = FMList;


                        foreach (var item in mModel.fMMasters)
                        {
                            var List = ctxTFAT.FMROUTETable.Where(x => x.Parentkey.ToString().Trim() == item.RefTablekey.Trim() && String.IsNullOrEmpty(x.ArrivalRemark) == false).ToList().Count;
                            if (List > 0)
                            {
                                item.NarrReq = true;
                            }

                            #region Check Schedule There Or NOt
                            decimal StartKM = 0, EndKM = 0;
                            bool ShowSchedule = false;
                            #region Start KM OF Vehicle

                            var FirstRoute = ctxTFAT.FMROUTETable.Where(x => x.Parentkey.ToString() == item.RefTablekey.ToString() && x.SequenceRoute == 0).ToList();
                            foreach (var route in FirstRoute)
                            {
                                if (route != null)
                                {
                                    if (route.ArrivalKM != null || route.DispatchKM != null)
                                    {
                                        if (route.ArrivalKM != null)
                                        {
                                            StartKM = route.ArrivalKM.Value;
                                        }
                                        else
                                        {
                                            StartKM = route.DispatchKM.Value;
                                        }
                                        break;
                                    }
                                }
                            }

                            var TotalRoute = ctxTFAT.FMROUTETable.Where(x => x.Parentkey.ToString() == item.RefTablekey.ToString() && x.RouteType == "R").ToList().Count() - 1;
                            var LastRoute = ctxTFAT.FMROUTETable.Where(x => x.Parentkey.ToString() == item.RefTablekey.ToString() && x.SubRoute == TotalRoute).OrderByDescending(x => x.RECORDKEY).FirstOrDefault();
                            if (LastRoute != null)
                            {
                                if (LastRoute.ArrivalReSchKm != null)
                                {
                                    EndKM = LastRoute.ArrivalReSchKm.Value;
                                }

                            }

                            var Count = ctxTFAT.FMROUTETable.Where(x => x.Parentkey.ToString() == item.RefTablekey.ToString() && String.IsNullOrEmpty(x.ArrivalSchTime) == false).ToList().Count();
                            if (Count > 0)
                            {
                                ShowSchedule = true;
                            }
                            item.ShowSchedule = ShowSchedule;
                            item.StartKM = StartKM;
                            item.EndKM = EndKM;
                            item.RunningKM = EndKM - StartKM;
                            #endregion
                            #endregion
                        }

                        TempData["SelecedFM"] = FMList;
                    }
                    else
                    {
                        var FMList = (from FreightMemo in ctxTFAT.TripFmList
                                      where FreightMemo.DocNo == mModel.Document && FreightMemo.Prefix == mList.Prefix
                                      orderby FreightMemo.FMNo
                                      select new FMMasterTrip()
                                      {
                                          RefTablekey = FreightMemo.RefTablekey,
                                          FMNo = FreightMemo.FMNo.ToString(),
                                          Date = FreightMemo.FmDate,
                                          VehicleNo = ctxTFAT.VehicleMaster.Where(x => x.Code == FreightMemo.VehicleNo).Select(x => x.TruckNo).FirstOrDefault(),

                                          From = ctxTFAT.TfatBranch.Where(x => x.Code == FreightMemo.FromBranch).Select(x => x.Name).FirstOrDefault(),
                                          FromC = FreightMemo.FromBranch,
                                          To = ctxTFAT.TfatBranch.Where(x => x.Code == FreightMemo.ToBranch).Select(x => x.Name).FirstOrDefault(),
                                          ToC = FreightMemo.ToBranch,
                                          RouteVia = "",
                                          RouteViaC = FreightMemo.RouteVia,
                                          Tripchages = FreightMemo.TripChrg,
                                          LocalCharges = FreightMemo.LocalChrg,
                                          ViaCharges = FreightMemo.ViaChrg,
                                          Total = FreightMemo.TripChrg + FreightMemo.LocalChrg + FreightMemo.ViaChrg,
                                          DieselLtr = ctxTFAT.LRMaster.Where(x => x.TableKey.ToString() == FreightMemo.RefTablekey).Select(x => x.DieselLtr ?? "0").FirstOrDefault(),
                                          //TripBal= FreightMemo.Total-(ctxTFAT.FMMaster.Where(x=>x.FmNo.ToString()==FreightMemo.FMNo).Select(x=>x.Adv).FirstOrDefault())
                                      }).ToList();
                        foreach (var item in FMList)
                        {
                            var Lrmaster = ctxTFAT.LRMaster.Where(x => x.TableKey == item.RefTablekey).FirstOrDefault();
                            item.RouteVia = ctxTFAT.CustomerMaster.Where(x => x.Code == Lrmaster.BillParty).Select(x => x.Name).FirstOrDefault();
                            item.RouteViaC = Lrmaster.BillParty;
                            item.Driver = ctxTFAT.DriverMaster.Where(x => x.Code == Lrmaster.Driver).Select(x => x.Name).FirstOrDefault();
                            item.DriverC = Lrmaster.Driver;
                        }
                        mModel.fMMasters = FMList;
                        TempData["SelecedLR"] = FMList;
                    }
                    #endregion

                    #region Expenses List
                    var ExpensesList = (from Expenses in ctxTFAT.TripExpensesMaster
                                        where Expenses.DocNo == mModel.Document && Expenses.CreditAmt == 0 && Expenses.Prefix == mList.Prefix
                                        orderby Expenses.DocRefCode
                                        select new OtherExpenses()
                                        {
                                            ExpensesAc = Expenses.Account,
                                            ExpensesAcName = ctxTFAT.Master.Where(x => x.Code == Expenses.Account).Select(x => x.Name).FirstOrDefault(),
                                            CostCenterTally = ctxTFAT.Master.Where(x => x.Code == Expenses.Account).Select(x => x.CostCenterAmtTally).FirstOrDefault(),
                                            DocNo = Expenses.DocRefCode,
                                            Amount = Expenses.DebitAmt,
                                            RelatedTo = Expenses.RefType,
                                            Branch = Expenses.Branch,
                                            Tablekey = Expenses.TableKey,
                                            Narr = Expenses.Narr,
                                        }).ToList();

                    foreach (var item in ExpensesList)
                    {
                        item.LRDetailList = GetLRDetailList(item.Branch + item.Tablekey);
                        item.FMDetailList = GetFMDetailList(item.Branch + item.Tablekey);
                        item.PartialDivName = item.LRDetailList.Count() > 0 ? "LR" : item.FMDetailList.Count() > 0 ? "FM" : "";
                    }
                    int i = 1;
                    foreach (var item in ExpensesList)
                    {
                        item.tempId = i++;
                    }

                    mModel.expenseslist = ExpensesList;
                    Session["OtherExpensesList"] = ExpensesList;
                    #endregion

                    #region Deduction List
                    var DeductionList = (from Deduction in ctxTFAT.TripExpensesMaster
                                         where Deduction.DocNo == mModel.Document && Deduction.DebitAmt == 0 && Deduction.Prefix == mperiod
                                         orderby Deduction.DocRefCode
                                         select new OtherExpenses()
                                         {
                                             ExpensesAc = Deduction.Account,
                                             ExpensesAcName = ctxTFAT.Master.Where(x => x.Code == Deduction.Account).Select(x => x.Name).FirstOrDefault(),
                                             CostCenterTally = ctxTFAT.Master.Where(x => x.Code == Deduction.Account).Select(x => x.CostCenterAmtTally).FirstOrDefault(),
                                             DocNo = Deduction.DocRefCode,
                                             Amount = Deduction.CreditAmt,
                                             RelatedTo = Deduction.RefType,
                                             Branch = Deduction.Branch,
                                             Tablekey = Deduction.TableKey,
                                             Narr = Deduction.Narr,
                                         }).ToList();

                    foreach (var item in DeductionList)
                    {
                        item.LRDetailList = GetLRDetailList(item.Branch + item.Tablekey);
                        item.FMDetailList = GetFMDetailList(item.Branch + item.Tablekey);
                        item.PartialDivName = item.LRDetailList.Count() > 0 ? "LR" : item.FMDetailList.Count() > 0 ? "FM" : "";
                    }
                    i = 1;
                    foreach (var item in DeductionList)
                    {
                        item.tempId = i++;
                    }

                    mModel.deductionlist = DeductionList;
                    Session["OtherDeductionList"] = DeductionList;
                    #endregion


                    var Exp = mModel.fMMasters.Sum(x => x.Total) + mModel.expenseslist.Sum(x => x.Amount) + mModel.TripChrgKMExp;
                    var Ded = mModel.deductionlist.Sum(x => x.Amount);
                    mModel.NetAmt = Exp - Ded;

                    #region Get TDS and Check Locking (PeriodLock/LockAuthorise)
                    var mLedger = ctxTFAT.Ledger.Where(x => x.Type == "Trip0" && x.Srl == mModel.Document && x.Prefix == mList.Prefix).OrderBy(x => x.Sno).FirstOrDefault();
                    if (mLedger != null)
                    {
                        if (mLedger.TDSFlag)
                        {

                            mModel.CutTDS = mLedger.TDSFlag;
                            mModel.TDSCode = mLedger.TDSCode == null ? "" : mLedger.TDSCode.Value.ToString();
                            mModel.TDSCodeName = ctxTFAT.TDSMaster.Where(x => x.Code.ToString() == mModel.TDSCode).Select(x => x.Name).FirstOrDefault();
                            if (mModel.NetAmt > 0)
                            {
                                mModel.TDSRate = Math.Abs(((decimal)mModel.TDSAmt * 100) / mModel.NetAmt);
                            }
                        }
                        mModel.PeriodLock = PeriodLock(mLedger.Branch, mLedger.Type, mLedger.DocDate);
                        if (mLedger.AUTHORISE.Substring(0, 1) == "A")
                        {
                            mModel.LockAuthorise = LockAuthorise(mLedger.Type, mModel.Mode, mLedger.ParentKey, mLedger.ParentKey);
                        }
                    }
                    #endregion

                }
            }
            else
            {
                mModel.TripSheetDate = DateTime.Now.ToShortDateString();
                mModel.DocDate = DateTime.Now;
                if (mModel.Pick_Financial_Document)
                {
                    mModel.FromDate = ConvertDDMMYYTOYYMMDD(StartDate).ToShortDateString();
                    mModel.TODate = ConvertDDMMYYTOYYMMDD(EndDate).ToShortDateString();
                }
                else
                {
                    mModel.FromDate = DateTime.Now.ToShortDateString();
                    mModel.TODate = DateTime.Now.ToShortDateString();
                }
                mModel.AdvFromDate = DateTime.Now.ToShortDateString();
                mModel.AdvTODate = DateTime.Now.ToShortDateString();
                mModel.BalFromDate = DateTime.Now.ToShortDateString();
                mModel.BalTODate = DateTime.Now.ToShortDateString();
                mModel.CCFromDate = DateTime.Now.ToShortDateString();
                mModel.CCTODate = DateTime.Now.ToShortDateString();
                mModel.FromKM = "0";
                mModel.ToKM = "0";
                mModel.RunningKM = "0";
                mModel.PerKMChrg = 0;
                mModel.TripChrgKMExp = 0;
                mModel.AdvCutFromSummary = true;

                mModel.FMOrNOt = TripSetup.TripFMDefaultDoc;
                mModel.VehicleFlag = TripSetup.DriverTripDefault == true ? false : true;
            }
            mModel.otherExpenses = new OtherExpenses();
            mModel.otherExpenses.RelatedTo = "Other";
            return View(mModel);
        }


        #region FmWise TripChares Fetching

        public ActionResult FetchFreight_Advance(string FMNo)
        {
            decimal TripCharges = 0, LocalCharges = 0, ViaCharges = 0, Payment = 0;
            string FromBranch = "", RouteVia = "", ToBranch = "", Vehicle = "", VehicleCategory = "", FMDate = "";
            FMMaster fMMaster = ctxTFAT.FMMaster.Where(x => x.TableKey.ToString() == FMNo).FirstOrDefault();
            if (fMMaster != null)
            {
                FromBranch = fMMaster.FromBranch;
                RouteVia = fMMaster.RouteVia;
                ToBranch = fMMaster.ToBranch;
                Vehicle = fMMaster.TruckNo;
                VehicleCategory = fMMaster.VehicleCategory;
                FMDate = fMMaster.Date.ToShortDateString();
                Payment = fMMaster.Adv;
            }

            if (!(String.IsNullOrEmpty(FMDate) || String.IsNullOrEmpty(Vehicle) || String.IsNullOrEmpty(VehicleCategory)))
            {
                bool CheckParent = false;
                VehicleMaster vehicleMaster = ctxTFAT.VehicleMaster.Where(x => x.Code.Trim() == Vehicle.Trim()).FirstOrDefault();
                if (vehicleMaster != null && vehicleMaster.PickVehicleRate == true)
                {
                    DateTime CurrentDate = ConvertDDMMYYTOYYMMDD(FMDate);
                    TfatBranch tfatBranchFrom = new TfatBranch();
                    TfatBranch tfatBranchTo = new TfatBranch();

                    if (vehicleMaster != null)
                    {
                        CheckParent = vehicleMaster.GetParentRateAlso;
                    }
                    if (CheckParent)
                    {
                        tfatBranchFrom = ctxTFAT.TfatBranch.Where(x => x.Code == FromBranch).FirstOrDefault();
                        if (tfatBranchFrom.Category == "Area" || tfatBranchFrom.Category == "SubBranch")
                        {
                            tfatBranchFrom = ctxTFAT.TfatBranch.Where(x => x.Code == tfatBranchFrom.Grp).FirstOrDefault();
                        }
                        tfatBranchTo = ctxTFAT.TfatBranch.Where(x => x.Code == ToBranch).FirstOrDefault();
                        if (tfatBranchTo.Category == "Area" || tfatBranchTo.Category == "SubBranch")
                        {
                            tfatBranchTo = ctxTFAT.TfatBranch.Where(x => x.Code == tfatBranchTo.Grp).FirstOrDefault();
                        }
                    }

                    bool GetLocalCharges = true;

                    #region Get TripCHarges

                    if (!(String.IsNullOrEmpty(FromBranch) && String.IsNullOrEmpty(ToBranch)))
                    {
                        var RateType = vehicleMaster.RateType == null ? "".Split(',').ToList() : vehicleMaster.RateType.Split(',').ToList();
                        if (RateType.Contains("V"))
                        {
                            //From TO
                            TripCharges = CatchCharge("VNo", Vehicle, FromBranch, CurrentDate, ToBranch);
                            if (TripCharges == 0)
                            {
                                //From-Parent => TO
                                TripCharges = CatchCharge("VNo", Vehicle, tfatBranchFrom.Code, CurrentDate, ToBranch);
                                if (TripCharges == 0)
                                {
                                    //From => TO-Parent
                                    TripCharges = CatchCharge("VNo", Vehicle, FromBranch, CurrentDate, tfatBranchTo.Code);
                                    if (TripCharges == 0)
                                    {
                                        //From-Parent => TO-Parent
                                        TripCharges = CatchCharge("VNo", Vehicle, FromBranch, CurrentDate, tfatBranchTo.Code);
                                        if (TripCharges == 0)
                                        {
                                            //Category Check
                                            if (RateType.Contains("C"))
                                            {
                                                //From TO
                                                TripCharges = CatchCharge("VCategory", VehicleCategory, FromBranch, CurrentDate, ToBranch);
                                                if (TripCharges == 0)
                                                {
                                                    //From-Parent => TO
                                                    TripCharges = CatchCharge("VCategory", VehicleCategory, tfatBranchFrom.Code, CurrentDate, ToBranch);
                                                    if (TripCharges == 0)
                                                    {
                                                        //From => TO-Parent
                                                        TripCharges = CatchCharge("VCategory", VehicleCategory, FromBranch, CurrentDate, tfatBranchTo.Code);
                                                        if (TripCharges == 0)
                                                        {
                                                            //From-Parent => TO-Parent
                                                            TripCharges = CatchCharge("VCategory", VehicleCategory, tfatBranchFrom.Code, CurrentDate, tfatBranchTo.Code);
                                                            if (FromBranch == ToBranch)
                                                            {
                                                                decimal LocalFreight = CatchLocalCharges("VCategory", VehicleCategory, FromBranch, CurrentDate);
                                                                LocalCharges += LocalFreight;
                                                            }
                                                            else
                                                            {
                                                                decimal LocalFreight = CatchLocalCharges("VCategory", VehicleCategory, FromBranch, CurrentDate);
                                                                LocalCharges += LocalFreight;
                                                                LocalFreight = CatchLocalCharges("VCategory", VehicleCategory, ToBranch, CurrentDate);
                                                                LocalCharges += LocalFreight;
                                                            }
                                                        }
                                                        else
                                                        {
                                                            decimal LocalFreight = CatchLocalCharges("VCategory", VehicleCategory, ToBranch, CurrentDate);
                                                            LocalCharges += LocalFreight;
                                                        }
                                                    }
                                                    else
                                                    {
                                                        decimal LocalFreight = CatchLocalCharges("VCategory", VehicleCategory, FromBranch, CurrentDate);
                                                        LocalCharges += LocalFreight;
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                if (FromBranch == ToBranch)
                                                {
                                                    decimal LocalFreight = CatchLocalCharges("VNo", Vehicle, FromBranch, CurrentDate);
                                                    LocalCharges += LocalFreight;
                                                }
                                                else
                                                {
                                                    decimal LocalFreight = CatchLocalCharges("VNo", Vehicle, FromBranch, CurrentDate);
                                                    LocalCharges += LocalFreight;
                                                    LocalFreight = CatchLocalCharges("VNo", Vehicle, ToBranch, CurrentDate);
                                                    LocalCharges += LocalFreight;
                                                }
                                            }
                                        }
                                    }
                                    else
                                    {
                                        decimal LocalFreight = CatchLocalCharges("VNo", Vehicle, ToBranch, CurrentDate);
                                        LocalCharges += LocalFreight;
                                    }
                                }
                                else
                                {
                                    decimal LocalFreight = CatchLocalCharges("VNo", Vehicle, FromBranch, CurrentDate);
                                    LocalCharges += LocalFreight;
                                }
                            }
                        }
                        else
                        {
                            if (RateType.Contains("C"))
                            {
                                //From TO
                                TripCharges = CatchCharge("VCategory", VehicleCategory, FromBranch, CurrentDate, ToBranch);
                                if (TripCharges == 0)
                                {
                                    //From-Parent => TO
                                    TripCharges = CatchCharge("VCategory", VehicleCategory, tfatBranchFrom.Code, CurrentDate, ToBranch);
                                    if (TripCharges == 0)
                                    {
                                        //From => TO-Parent
                                        TripCharges = CatchCharge("VCategory", VehicleCategory, FromBranch, CurrentDate, tfatBranchTo.Code);
                                        if (TripCharges == 0)
                                        {
                                            //From-Parent => TO-Parent
                                            TripCharges = CatchCharge("VCategory", VehicleCategory, tfatBranchFrom.Code, CurrentDate, tfatBranchTo.Code);
                                            if (FromBranch == ToBranch)
                                            {
                                                decimal LocalFreight = CatchLocalCharges("VCategory", VehicleCategory, FromBranch, CurrentDate);
                                                LocalCharges += LocalFreight;
                                            }
                                            else
                                            {
                                                decimal LocalFreight = CatchLocalCharges("VCategory", VehicleCategory, FromBranch, CurrentDate);
                                                LocalCharges += LocalFreight;
                                                LocalFreight = CatchLocalCharges("VCategory", VehicleCategory, ToBranch, CurrentDate);
                                                LocalCharges += LocalFreight;
                                            }
                                        }
                                        else
                                        {
                                            decimal LocalFreight = CatchLocalCharges("VCategory", VehicleCategory, ToBranch, CurrentDate);
                                            LocalCharges += LocalFreight;
                                        }
                                    }
                                    else
                                    {
                                        decimal LocalFreight = CatchLocalCharges("VCategory", VehicleCategory, FromBranch, CurrentDate);
                                        LocalCharges += LocalFreight;
                                    }
                                }
                            }
                        }
                    }

                    #endregion



                    #region Get ViaCharges

                    if (!String.IsNullOrEmpty(RouteVia))
                    {
                        var RouteList = RouteVia.Split(',');
                        foreach (var item in RouteList)
                        {
                            ViaCharges = CatchViaCharges("VNo", Vehicle, item, CurrentDate);
                            if (ViaCharges == 0)
                            {
                                ViaCharges = CatchViaCharges("VCategory", VehicleCategory, item, CurrentDate);
                            }
                            ViaCharges += ViaCharges;
                        }

                    }

                    #endregion

                    #region Catch Payment Of FM

                    //Payment= CatchFMPayment(fMMaster.FmNo.ToString(), fMMaster.Branch,  fMMaster.DriverPostAc);


                    #endregion
                }
                else
                {
                    HireVehicleMaster hireVehicleMaster = ctxTFAT.HireVehicleMaster.Where(x => x.Code.Trim() == Vehicle.Trim()).FirstOrDefault();
                    if (hireVehicleMaster != null)
                    {
                        DateTime CurrentDate = ConvertDDMMYYTOYYMMDD(FMDate);
                        TfatBranch tfatBranchFrom = new TfatBranch();
                        TfatBranch tfatBranchTo = new TfatBranch();

                        if (hireVehicleMaster != null)
                        {
                            CheckParent = hireVehicleMaster.GetParentRateAlso;
                        }
                        if (CheckParent)
                        {
                            tfatBranchFrom = ctxTFAT.TfatBranch.Where(x => x.Code == FromBranch).FirstOrDefault();
                            if (tfatBranchFrom.Category == "Area" || tfatBranchFrom.Category == "SubBranch")
                            {
                                tfatBranchFrom = ctxTFAT.TfatBranch.Where(x => x.Code == tfatBranchFrom.Grp).FirstOrDefault();
                            }
                            tfatBranchTo = ctxTFAT.TfatBranch.Where(x => x.Code == ToBranch).FirstOrDefault();
                            if (tfatBranchTo.Category == "Area" || tfatBranchTo.Category == "SubBranch")
                            {
                                tfatBranchTo = ctxTFAT.TfatBranch.Where(x => x.Code == tfatBranchTo.Grp).FirstOrDefault();
                            }
                        }

                        bool GetLocalCharges = true;

                        #region Get TripCHarges

                        if (!(String.IsNullOrEmpty(FromBranch) && String.IsNullOrEmpty(ToBranch)))
                        {
                            var RateType = hireVehicleMaster.RateType.Split(',').ToList();
                            if (RateType.Contains("V"))
                            {
                                //From TO
                                TripCharges = CatchCharge("VNo", Vehicle, FromBranch, CurrentDate, ToBranch);
                                if (TripCharges == 0)
                                {
                                    //From-Parent => TO
                                    TripCharges = CatchCharge("VNo", Vehicle, tfatBranchFrom.Code, CurrentDate, ToBranch);
                                    if (TripCharges == 0)
                                    {
                                        //From => TO-Parent
                                        TripCharges = CatchCharge("VNo", Vehicle, FromBranch, CurrentDate, tfatBranchTo.Code);
                                        if (TripCharges == 0)
                                        {
                                            //From-Parent => TO-Parent
                                            TripCharges = CatchCharge("VNo", Vehicle, FromBranch, CurrentDate, tfatBranchTo.Code);
                                            if (TripCharges == 0)
                                            {
                                                //Category Check
                                                if (RateType.Contains("C"))
                                                {
                                                    //From TO
                                                    TripCharges = CatchCharge("VCategory", VehicleCategory, FromBranch, CurrentDate, ToBranch);
                                                    if (TripCharges == 0)
                                                    {
                                                        //From-Parent => TO
                                                        TripCharges = CatchCharge("VCategory", VehicleCategory, tfatBranchFrom.Code, CurrentDate, ToBranch);
                                                        if (TripCharges == 0)
                                                        {
                                                            //From => TO-Parent
                                                            TripCharges = CatchCharge("VCategory", VehicleCategory, FromBranch, CurrentDate, tfatBranchTo.Code);
                                                            if (TripCharges == 0)
                                                            {
                                                                //From-Parent => TO-Parent
                                                                TripCharges = CatchCharge("VCategory", VehicleCategory, tfatBranchFrom.Code, CurrentDate, tfatBranchTo.Code);
                                                                if (FromBranch == ToBranch)
                                                                {
                                                                    decimal LocalFreight = CatchLocalCharges("VCategory", VehicleCategory, FromBranch, CurrentDate);
                                                                    LocalCharges += LocalFreight;
                                                                }
                                                                else
                                                                {
                                                                    decimal LocalFreight = CatchLocalCharges("VCategory", VehicleCategory, FromBranch, CurrentDate);
                                                                    LocalCharges += LocalFreight;
                                                                    LocalFreight = CatchLocalCharges("VCategory", VehicleCategory, ToBranch, CurrentDate);
                                                                    LocalCharges += LocalFreight;
                                                                }
                                                            }
                                                            else
                                                            {
                                                                decimal LocalFreight = CatchLocalCharges("VCategory", VehicleCategory, ToBranch, CurrentDate);
                                                                LocalCharges += LocalFreight;
                                                            }
                                                        }
                                                        else
                                                        {
                                                            decimal LocalFreight = CatchLocalCharges("VCategory", VehicleCategory, FromBranch, CurrentDate);
                                                            LocalCharges += LocalFreight;
                                                        }
                                                    }
                                                }
                                                else
                                                {
                                                    if (FromBranch == ToBranch)
                                                    {
                                                        decimal LocalFreight = CatchLocalCharges("VNo", Vehicle, FromBranch, CurrentDate);
                                                        LocalCharges += LocalFreight;
                                                    }
                                                    else
                                                    {
                                                        decimal LocalFreight = CatchLocalCharges("VNo", Vehicle, FromBranch, CurrentDate);
                                                        LocalCharges += LocalFreight;
                                                        LocalFreight = CatchLocalCharges("VNo", Vehicle, ToBranch, CurrentDate);
                                                        LocalCharges += LocalFreight;
                                                    }
                                                }
                                            }
                                        }
                                        else
                                        {
                                            decimal LocalFreight = CatchLocalCharges("VNo", Vehicle, ToBranch, CurrentDate);
                                            LocalCharges += LocalFreight;
                                        }
                                    }
                                    else
                                    {
                                        decimal LocalFreight = CatchLocalCharges("VNo", Vehicle, FromBranch, CurrentDate);
                                        LocalCharges += LocalFreight;
                                    }
                                }
                            }
                            else
                            {
                                if (RateType.Contains("C"))
                                {
                                    //From TO
                                    TripCharges = CatchCharge("VCategory", VehicleCategory, FromBranch, CurrentDate, ToBranch);
                                    if (TripCharges == 0)
                                    {
                                        //From-Parent => TO
                                        TripCharges = CatchCharge("VCategory", VehicleCategory, tfatBranchFrom.Code, CurrentDate, ToBranch);
                                        if (TripCharges == 0)
                                        {
                                            //From => TO-Parent
                                            TripCharges = CatchCharge("VCategory", VehicleCategory, FromBranch, CurrentDate, tfatBranchTo.Code);
                                            if (TripCharges == 0)
                                            {
                                                //From-Parent => TO-Parent
                                                TripCharges = CatchCharge("VCategory", VehicleCategory, tfatBranchFrom.Code, CurrentDate, tfatBranchTo.Code);
                                                if (FromBranch == ToBranch)
                                                {
                                                    decimal LocalFreight = CatchLocalCharges("VCategory", VehicleCategory, FromBranch, CurrentDate);
                                                    LocalCharges += LocalFreight;
                                                }
                                                else
                                                {
                                                    decimal LocalFreight = CatchLocalCharges("VCategory", VehicleCategory, FromBranch, CurrentDate);
                                                    LocalCharges += LocalFreight;
                                                    LocalFreight = CatchLocalCharges("VCategory", VehicleCategory, ToBranch, CurrentDate);
                                                    LocalCharges += LocalFreight;
                                                }
                                            }
                                            else
                                            {
                                                decimal LocalFreight = CatchLocalCharges("VCategory", VehicleCategory, ToBranch, CurrentDate);
                                                LocalCharges += LocalFreight;
                                            }
                                        }
                                        else
                                        {
                                            decimal LocalFreight = CatchLocalCharges("VCategory", VehicleCategory, FromBranch, CurrentDate);
                                            LocalCharges += LocalFreight;
                                        }
                                    }
                                }
                            }
                        }

                        #endregion



                        #region Get ViaCharges

                        if (!String.IsNullOrEmpty(RouteVia))
                        {
                            var RouteList = RouteVia.Split(',');
                            foreach (var item in RouteList)
                            {
                                ViaCharges = CatchViaCharges("VNo", Vehicle, item, CurrentDate);
                                if (ViaCharges == 0)
                                {
                                    ViaCharges = CatchViaCharges("VCategory", VehicleCategory, item, CurrentDate);
                                }
                                ViaCharges += ViaCharges;
                            }

                        }

                        #endregion

                        #region Catch Payment Of FM

                        //Payment= CatchFMPayment(fMMaster.FmNo.ToString(), fMMaster.Branch,  fMMaster.DriverPostAc);


                        #endregion
                    }
                }
                return Json(new
                {
                    Status = "Success",
                    TripCharges = TripCharges,
                    LocalCharges = LocalCharges,
                    ViaCharges = ViaCharges,
                    PaymentAmt = Payment,
                    JsonRequestBehavior.AllowGet
                });
            }
            else
            {
                return Json(new
                {
                    Status = "Error",
                    TripCharges = TripCharges,
                    LocalCharges = LocalCharges,
                    ViaCharges = ViaCharges,
                    PaymentAmt = Payment,
                    JsonRequestBehavior.AllowGet
                });
            }
        }

        public decimal CatchCharge(string VehicleType, string Vehicle, string FromBranch, DateTime CurrentDate, string ToBranch)
        {
            decimal Freight = 0;
            TripChargesMa freightCharge = ctxTFAT.TripChargesMa.Where(x => x.VehicleType == VehicleType && x.Vehicle == Vehicle.Trim() && x.FromBranch == FromBranch && x.FromPeriod <= CurrentDate).OrderByDescending(x => x.FromPeriod).FirstOrDefault();
            if (freightCharge != null)
            {
                TripChargesMaRef freightChargeMaRef = ctxTFAT.TripChargesMaRef.Where(x => x.DocNo == freightCharge.DocNo && x.FromBranch == FromBranch && x.ToBranch == ToBranch).FirstOrDefault();
                if (freightChargeMaRef != null)
                {
                    Freight = freightChargeMaRef.TripCharge.Value;
                }
            }
            return Freight;
        }

        public decimal CatchLocalCharges(string VehicleType, string Vehicle, string FromBranch, DateTime CurrentDate)
        {
            decimal Freight = 0;
            LocalChargesMa freightCharge = ctxTFAT.LocalChargesMa.Where(x => x.VehicleType == VehicleType && x.Vehicle == Vehicle.Trim() && x.FromPeriod <= CurrentDate).OrderByDescending(x => x.FromPeriod).FirstOrDefault();
            if (freightCharge != null)
            {
                LocalChargesMaRef freightChargeMaRef = ctxTFAT.LocalChargesMaRef.Where(x => x.DocNo == freightCharge.DocNo && x.Area == FromBranch).FirstOrDefault();
                if (freightChargeMaRef != null)
                {
                    Freight = freightChargeMaRef.LocalCharges.Value;
                }
            }
            return Freight;
        }

        public decimal CatchViaCharges(string VehicleType, string Vehicle, string RouteVia, DateTime CurrentDate)
        {
            //var ListRouteVia = RouteVia.Split(',');
            decimal ViaAdvance = 0;
            ViaChargesMa freightCharge = ctxTFAT.ViaChargesMa.Where(x => x.VehicleType == VehicleType && x.Vehicle == Vehicle.Trim() && x.FromPeriod <= CurrentDate).OrderByDescending(x => x.FromPeriod).FirstOrDefault();
            if (freightCharge != null)
            {
                ViaChargesMaRef freightChargeMaRef = ctxTFAT.ViaChargesMaRef.Where(x => x.DocNo == freightCharge.DocNo && x.Area == RouteVia).FirstOrDefault();
                if (freightChargeMaRef != null)
                {
                    ViaAdvance += freightChargeMaRef.ViaCharges.Value;
                }

            }
            return ViaAdvance;
        }

        public decimal FetchTripCharges(string VehicleType, string Vehicle, string FromBranch, DateTime CurrentDate, string ToBranch)
        {
            decimal Freight = 0;
            Freight_Trip_Adv freight_Trip_ = ctxTFAT.Freight_Trip_Adv.Where(x => x.VehicleType == VehicleType && x.Vehicle == Vehicle.Trim() && x.FromBranch == FromBranch && x.FromPeriod <= CurrentDate).OrderByDescending(x => x.FromPeriod).FirstOrDefault();
            if (freight_Trip_ != null)
            {
                Freight_Trip_AdvRef freight_Trip_Adv = ctxTFAT.Freight_Trip_AdvRef.Where(x => x.DocNo == freight_Trip_.DocNo && x.FromBranch == FromBranch && x.ToBranch == ToBranch).FirstOrDefault();
                if (freight_Trip_Adv != null)
                {
                    Freight = freight_Trip_Adv.TripCharge.Value;
                }
            }

            return Freight;
        }

        public decimal CatchFMPayment(string FMno, string FMbranch, string DriverCode)
        {
            decimal Freight = 0;
            var PaymentList = ctxTFAT.Ledger.Where(x => x.Branch == FMbranch && x.Code == DriverCode.Trim() && x.Srl == FMno).ToList();
            if (PaymentList != null)
            {
                Freight = PaymentList.Sum(x => (decimal?)x.Debit ?? 0);
            }
            return Freight;
        }

        #endregion

        #region Fetch LR Expenses

        public ActionResult FetchLRTripExp_Advance(string LRNO)
        {
            decimal TripCharges = 0, LocalCharges = 0, ViaCharges = 0, Payment = 0;
            string FromBranch = "", RouteVia = "", ToBranch = "", Vehicle = "", VehicleCategory = "", FMDate = "";
            LRMaster lRMaster = ctxTFAT.LRMaster.Where(x => x.TableKey.ToString() == LRNO).FirstOrDefault();
            if (lRMaster != null)
            {
                TripCharges = Convert.ToDecimal(lRMaster.DriverTripExp == null ? 0 : lRMaster.DriverTripExp);
                return Json(new
                {
                    Status = "Success",
                    TripCharges = TripCharges,
                    LocalCharges = LocalCharges,
                    ViaCharges = ViaCharges,
                    PaymentAmt = String.IsNullOrEmpty(lRMaster.DieselLtr) == true ? "0" : lRMaster.DieselLtr,
                    JsonRequestBehavior.AllowGet
                });
            }
            else
            {
                return Json(new
                {
                    Status = "Error",
                    TripCharges = TripCharges,
                    LocalCharges = LocalCharges,
                    ViaCharges = ViaCharges,
                    PaymentAmt = Payment,
                    JsonRequestBehavior.AllowGet
                });
            }
        }

        #endregion


        #region FirstTab Using Function(FMList)

        public ActionResult GetFMList(TripSheetVM mModel)//Fetch Pending FM Related To Driver
        {
            mModel.FMOrNOt = true;
            List<FMMasterTrip> AllStockFMLIst = new List<FMMasterTrip>();
            var GetFmFrom = ctxTFAT.TripSheetSetup.Select(x => x.FetchFmFrom).FirstOrDefault();
            var StartFrom = ConvertDDMMYYTOYYMMDD(mModel.FromDate);
            var End = ConvertDDMMYYTOYYMMDD(mModel.TODate);
            if (mModel.Pick_Financial_Document == false && GetFmFrom.HasValue == false)
            {
                GetFmFrom = ConvertDDMMYYTOYYMMDD("01/01/2000");
            }

            if (GetFmFrom != null)
            {
                if (mModel.VehicleFlag)
                {
                    AllStockFMLIst = (from FMMaster in ctxTFAT.FMMaster
                                      where FMMaster.TruckNo == mModel.VehicleCode && FMMaster.Date >= GetFmFrom.Value
                                      orderby FMMaster.FmNo
                                      select new FMMasterTrip()
                                      {
                                          FMNo = FMMaster.FmNo.ToString(),
                                          Date = FMMaster.Date,
                                          VehicleNo = FMMaster.VehicleStatus == "100001" ? ctxTFAT.HireVehicleMaster.Where(x => x.Code == FMMaster.TruckNo).Select(x => x.TruckNo).FirstOrDefault() : ctxTFAT.VehicleMaster.Where(x => x.Code == FMMaster.TruckNo).Select(x => x.TruckNo).FirstOrDefault(),
                                          Driver = ctxTFAT.DriverMaster.Where(x => x.Code == FMMaster.Driver).Select(x => x.Name).FirstOrDefault(),
                                          DriverC = FMMaster.Driver,
                                          From = ctxTFAT.TfatBranch.Where(x => x.Code == FMMaster.FromBranch).Select(x => x.Name).FirstOrDefault(),
                                          FromC = FMMaster.FromBranch,
                                          To = ctxTFAT.TfatBranch.Where(x => x.Code == FMMaster.ToBranch).Select(x => x.Name).FirstOrDefault(),
                                          ToC = FMMaster.ToBranch,
                                          RouteVia = FMMaster.RouteViaName,
                                          RouteViaC = FMMaster.RouteVia,
                                          RefTablekey = FMMaster.TableKey,
                                          Tripchages = 0,
                                          LocalCharges = 0,
                                          ViaCharges = 0,
                                          PaymentAmt = 0,
                                          Total = 0,
                                          DieselLtr = String.IsNullOrEmpty(FMMaster.DieselLtr) == true ? "0" : FMMaster.DieselLtr,
                                      }).ToList();
                }
                else
                {
                    AllStockFMLIst = (from FMMaster in ctxTFAT.FMMaster
                                      where FMMaster.Driver == mModel.DriverCode && FMMaster.Date >= GetFmFrom.Value
                                      orderby FMMaster.FmNo
                                      select new FMMasterTrip()
                                      {
                                          FMNo = FMMaster.FmNo.ToString(),
                                          Date = FMMaster.Date,
                                          VehicleNo = FMMaster.VehicleStatus == "100001" ? ctxTFAT.HireVehicleMaster.Where(x => x.Code == FMMaster.TruckNo).Select(x => x.TruckNo).FirstOrDefault() : ctxTFAT.VehicleMaster.Where(x => x.Code == FMMaster.TruckNo).Select(x => x.TruckNo).FirstOrDefault(),
                                          Driver = ctxTFAT.DriverMaster.Where(x => x.Code == FMMaster.Driver).Select(x => x.Name).FirstOrDefault(),
                                          DriverC = FMMaster.Driver,
                                          From = ctxTFAT.TfatBranch.Where(x => x.Code == FMMaster.FromBranch).Select(x => x.Name).FirstOrDefault(),
                                          FromC = FMMaster.FromBranch,
                                          To = ctxTFAT.TfatBranch.Where(x => x.Code == FMMaster.ToBranch).Select(x => x.Name).FirstOrDefault(),
                                          ToC = FMMaster.ToBranch,
                                          RouteVia = FMMaster.RouteViaName,
                                          RouteViaC = FMMaster.RouteVia,
                                          RefTablekey = FMMaster.TableKey,
                                          Tripchages = 0,
                                          LocalCharges = 0,
                                          ViaCharges = 0,
                                          PaymentAmt = 0,
                                          Total = 0,
                                          DieselLtr = String.IsNullOrEmpty(FMMaster.DieselLtr) == true ? "0" : FMMaster.DieselLtr,
                                      }).ToList();
                }

            }
            else
            {

                if (mModel.VehicleFlag)
                {
                    AllStockFMLIst = (from FMMaster in ctxTFAT.FMMaster
                                      where FMMaster.TruckNo == mModel.VehicleCode && FMMaster.Date >= StartFrom && FMMaster.Date <= End
                                      orderby FMMaster.FmNo
                                      select new FMMasterTrip()
                                      {
                                          RefTablekey = FMMaster.TableKey,
                                          FMNo = FMMaster.FmNo.ToString(),
                                          Date = FMMaster.Date,
                                          VehicleNo = FMMaster.VehicleStatus == "100001" ? ctxTFAT.HireVehicleMaster.Where(x => x.Code == FMMaster.TruckNo).Select(x => x.TruckNo).FirstOrDefault() : ctxTFAT.VehicleMaster.Where(x => x.Code == FMMaster.TruckNo).Select(x => x.TruckNo).FirstOrDefault(),
                                          Driver = ctxTFAT.DriverMaster.Where(x => x.Code == FMMaster.Driver).Select(x => x.Name).FirstOrDefault(),
                                          DriverC = FMMaster.Driver,
                                          From = ctxTFAT.TfatBranch.Where(x => x.Code == FMMaster.FromBranch).Select(x => x.Name).FirstOrDefault(),
                                          FromC = FMMaster.FromBranch,
                                          To = ctxTFAT.TfatBranch.Where(x => x.Code == FMMaster.ToBranch).Select(x => x.Name).FirstOrDefault(),
                                          ToC = FMMaster.ToBranch,
                                          RouteVia = FMMaster.RouteViaName,
                                          RouteViaC = FMMaster.RouteVia,
                                          Tripchages = 0,
                                          LocalCharges = 0,
                                          ViaCharges = 0,
                                          PaymentAmt = 0,
                                          Total = 0,
                                          DieselLtr = String.IsNullOrEmpty(FMMaster.DieselLtr) == true ? "0" : FMMaster.DieselLtr,
                                      }).ToList();
                }
                else
                {
                    AllStockFMLIst = (from FMMaster in ctxTFAT.FMMaster
                                      where FMMaster.Driver == mModel.DriverCode && FMMaster.Date >= StartFrom && FMMaster.Date <= End
                                      orderby FMMaster.FmNo
                                      select new FMMasterTrip()
                                      {
                                          RefTablekey = FMMaster.TableKey,
                                          FMNo = FMMaster.FmNo.ToString(),
                                          Date = FMMaster.Date,
                                          VehicleNo = FMMaster.VehicleStatus == "100001" ? ctxTFAT.HireVehicleMaster.Where(x => x.Code == FMMaster.TruckNo).Select(x => x.TruckNo).FirstOrDefault() : ctxTFAT.VehicleMaster.Where(x => x.Code == FMMaster.TruckNo).Select(x => x.TruckNo).FirstOrDefault(),
                                          Driver = ctxTFAT.DriverMaster.Where(x => x.Code == FMMaster.Driver).Select(x => x.Name).FirstOrDefault(),
                                          DriverC = FMMaster.Driver,
                                          From = ctxTFAT.TfatBranch.Where(x => x.Code == FMMaster.FromBranch).Select(x => x.Name).FirstOrDefault(),
                                          FromC = FMMaster.FromBranch,
                                          To = ctxTFAT.TfatBranch.Where(x => x.Code == FMMaster.ToBranch).Select(x => x.Name).FirstOrDefault(),
                                          ToC = FMMaster.ToBranch,
                                          RouteVia = FMMaster.RouteViaName,
                                          RouteViaC = FMMaster.RouteVia,
                                          Tripchages = 0,
                                          LocalCharges = 0,
                                          ViaCharges = 0,
                                          PaymentAmt = 0,
                                          Total = 0,
                                          DieselLtr = String.IsNullOrEmpty(FMMaster.DieselLtr) == true ? "0" : FMMaster.DieselLtr,
                                      }).ToList();
                }
            }

            List<FMMasterTrip> SelectedFm = TempData.Peek("SelecedFM") as List<FMMasterTrip>;
            if (SelectedFm == null)
            {
                SelectedFm = new List<FMMasterTrip>();
            }
            var FetSelectedFm = SelectedFm.Select(x => x.RefTablekey).ToList();
            AllStockFMLIst = AllStockFMLIst.Where(x => !(FetSelectedFm.Contains(x.RefTablekey))).ToList();
            var CreatedTripFmList = ctxTFAT.TripFmList.Select(x => x.RefTablekey).ToList();
            AllStockFMLIst = AllStockFMLIst.Where(x => !(CreatedTripFmList.Contains(x.RefTablekey))).ToList();

            mModel.fMMasters = AllStockFMLIst.OrderBy(x => x.Date).ToList();
            TempData["PendingFM"] = AllStockFMLIst;
            var html = ViewHelper.RenderPartialView(this, "FMListCombo", mModel);
            var jsonResult = Json(new { Html = html }, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue;
            return jsonResult;
        }
        public ActionResult GetLRList(TripSheetVM mModel)//Fetch Pending FM Related To Driver
        {
            mModel.FMOrNOt = false;
            List<FMMasterTrip> AllStockFMLIst = new List<FMMasterTrip>();
            var GetFmFrom = ctxTFAT.TripSheetSetup.Select(x => x.FetchFmFrom).FirstOrDefault();
            var StartFrom = ConvertDDMMYYTOYYMMDD(mModel.FromDate);
            var End = ConvertDDMMYYTOYYMMDD(mModel.TODate);
            if (mModel.Pick_Financial_Document == false && GetFmFrom.HasValue == false)
            {
                GetFmFrom = ConvertDDMMYYTOYYMMDD("01/01/2000");
            }
            if (GetFmFrom != null)
            {
                if (mModel.VehicleFlag)
                {
                    AllStockFMLIst = (from FMMaster in ctxTFAT.LRMaster
                                      where FMMaster.VehicleNo == mModel.VehicleCode && FMMaster.BookDate >= GetFmFrom.Value
                                      orderby FMMaster.LrNo
                                      select new FMMasterTrip()
                                      {
                                          RefTablekey = FMMaster.TableKey,
                                          FMNo = FMMaster.LrNo.ToString(),
                                          Date = FMMaster.BookDate,
                                          VehicleNo = ctxTFAT.VehicleMaster.Where(x => x.Code == FMMaster.VehicleNo).Select(x => x.TruckNo).FirstOrDefault(),
                                          Driver = ctxTFAT.DriverMaster.Where(x => x.Code == FMMaster.Driver).Select(x => x.Name).FirstOrDefault(),
                                          DriverC = FMMaster.Driver,
                                          From = ctxTFAT.TfatBranch.Where(x => x.Code == FMMaster.Source).Select(x => x.Name).FirstOrDefault(),
                                          FromC = FMMaster.Source,
                                          To = ctxTFAT.TfatBranch.Where(x => x.Code == FMMaster.Dest).Select(x => x.Name).FirstOrDefault(),
                                          ToC = FMMaster.Dest,
                                          RouteVia = ctxTFAT.CustomerMaster.Where(x => x.Code == FMMaster.BillParty).Select(x => x.Name).FirstOrDefault(),
                                          RouteViaC = FMMaster.BillParty,
                                          Tripchages = 0,
                                          LocalCharges = 0,
                                          ViaCharges = 0,
                                          DieselLtr = String.IsNullOrEmpty(FMMaster.DieselLtr) == true ? "0" : FMMaster.DieselLtr,
                                          Total = 0
                                      }).ToList();
                }
                else
                {
                    AllStockFMLIst = (from FMMaster in ctxTFAT.LRMaster
                                      where FMMaster.Driver == mModel.DriverCode && FMMaster.BookDate >= GetFmFrom.Value
                                      orderby FMMaster.LrNo
                                      select new FMMasterTrip()
                                      {
                                          RefTablekey = FMMaster.TableKey,
                                          FMNo = FMMaster.LrNo.ToString(),
                                          Date = FMMaster.BookDate,
                                          VehicleNo = ctxTFAT.VehicleMaster.Where(x => x.Code == FMMaster.VehicleNo).Select(x => x.TruckNo).FirstOrDefault(),
                                          Driver = ctxTFAT.DriverMaster.Where(x => x.Code == FMMaster.Driver).Select(x => x.Name).FirstOrDefault(),
                                          DriverC = FMMaster.Driver,
                                          From = ctxTFAT.TfatBranch.Where(x => x.Code == FMMaster.Source).Select(x => x.Name).FirstOrDefault(),
                                          FromC = FMMaster.Source,
                                          To = ctxTFAT.TfatBranch.Where(x => x.Code == FMMaster.Dest).Select(x => x.Name).FirstOrDefault(),
                                          ToC = FMMaster.Dest,
                                          RouteVia = ctxTFAT.CustomerMaster.Where(x => x.Code == FMMaster.BillParty).Select(x => x.Name).FirstOrDefault(),
                                          RouteViaC = FMMaster.BillParty,
                                          Tripchages = 0,
                                          LocalCharges = 0,
                                          ViaCharges = 0,
                                          DieselLtr = String.IsNullOrEmpty(FMMaster.DieselLtr) == true ? "0" : FMMaster.DieselLtr,
                                          Total = 0
                                      }).ToList();
                }
            }
            else
            {
                if (mModel.VehicleFlag)
                {
                    AllStockFMLIst = (from FMMaster in ctxTFAT.LRMaster
                                      where FMMaster.VehicleNo == mModel.VehicleCode && FMMaster.BookDate >= StartFrom && FMMaster.BookDate <= End
                                      orderby FMMaster.LrNo
                                      select new FMMasterTrip()
                                      {
                                          RefTablekey = FMMaster.TableKey,
                                          FMNo = FMMaster.LrNo.ToString(),
                                          Date = FMMaster.BookDate,
                                          VehicleNo = ctxTFAT.VehicleMaster.Where(x => x.Code == FMMaster.VehicleNo).Select(x => x.TruckNo).FirstOrDefault(),
                                          Driver = ctxTFAT.DriverMaster.Where(x => x.Code == FMMaster.Driver).Select(x => x.Name).FirstOrDefault(),
                                          DriverC = FMMaster.Driver,
                                          From = ctxTFAT.TfatBranch.Where(x => x.Code == FMMaster.Source).Select(x => x.Name).FirstOrDefault(),
                                          FromC = FMMaster.Source,
                                          To = ctxTFAT.TfatBranch.Where(x => x.Code == FMMaster.Dest).Select(x => x.Name).FirstOrDefault(),
                                          ToC = FMMaster.Dest,
                                          RouteVia = ctxTFAT.CustomerMaster.Where(x => x.Code == FMMaster.BillParty).Select(x => x.Name).FirstOrDefault(),
                                          RouteViaC = FMMaster.BillParty,
                                          Tripchages = 0,
                                          LocalCharges = 0,
                                          ViaCharges = 0,
                                          DieselLtr = String.IsNullOrEmpty(FMMaster.DieselLtr) == true ? "0" : FMMaster.DieselLtr,
                                          Total = 0
                                      }).ToList();
                }
                else
                {
                    AllStockFMLIst = (from FMMaster in ctxTFAT.LRMaster
                                      where FMMaster.Driver == mModel.DriverCode && FMMaster.BookDate >= StartFrom && FMMaster.BookDate <= End
                                      orderby FMMaster.LrNo
                                      select new FMMasterTrip()
                                      {
                                          RefTablekey = FMMaster.TableKey,
                                          FMNo = FMMaster.LrNo.ToString(),
                                          Date = FMMaster.BookDate,
                                          VehicleNo = ctxTFAT.VehicleMaster.Where(x => x.Code == FMMaster.VehicleNo).Select(x => x.TruckNo).FirstOrDefault(),
                                          Driver = ctxTFAT.DriverMaster.Where(x => x.Code == FMMaster.Driver).Select(x => x.Name).FirstOrDefault(),
                                          DriverC = FMMaster.Driver,
                                          From = ctxTFAT.TfatBranch.Where(x => x.Code == FMMaster.Source).Select(x => x.Name).FirstOrDefault(),
                                          FromC = FMMaster.Source,
                                          To = ctxTFAT.TfatBranch.Where(x => x.Code == FMMaster.Dest).Select(x => x.Name).FirstOrDefault(),
                                          ToC = FMMaster.Dest,
                                          RouteVia = ctxTFAT.CustomerMaster.Where(x => x.Code == FMMaster.BillParty).Select(x => x.Name).FirstOrDefault(),
                                          RouteViaC = FMMaster.BillParty,
                                          Tripchages = 0,
                                          LocalCharges = 0,
                                          ViaCharges = 0,
                                          DieselLtr = String.IsNullOrEmpty(FMMaster.DieselLtr) == true ? "0" : FMMaster.DieselLtr,
                                          Total = 0
                                      }).ToList();
                }
            }

            List<FMMasterTrip> SelectedFm = TempData.Peek("SelecedLR") as List<FMMasterTrip>;
            if (SelectedFm == null)
            {
                SelectedFm = new List<FMMasterTrip>();
            }
            var FetSelectedFm = SelectedFm.Select(x => x.RefTablekey).ToList();
            AllStockFMLIst = AllStockFMLIst.Where(x => !(FetSelectedFm.Contains(x.RefTablekey))).ToList();

            var CreatedTripFmList = ctxTFAT.TripFmList.Select(x => x.RefTablekey).ToList();
            AllStockFMLIst = AllStockFMLIst.Where(x => !(CreatedTripFmList.Contains(x.RefTablekey))).ToList();

            mModel.fMMasters = AllStockFMLIst;
            TempData["PendingLR"] = AllStockFMLIst;
            var html = ViewHelper.RenderPartialView(this, "FMListCombo", mModel);
            var jsonResult = Json(new { Html = html }, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue;
            return jsonResult;
        }

        public ActionResult SearchFMList(TripSheetVM mModel)//Fetch Pending FM Related To Driver
        {
            mModel.FMOrNOt = true;
            List<FMMasterTrip> AllStockFMLIst = new List<FMMasterTrip>();
            var GetFmFrom = ctxTFAT.TripSheetSetup.Select(x => x.FetchFmFrom).FirstOrDefault();
            if (GetFmFrom != null)
            {
                AllStockFMLIst = (from FMMaster in ctxTFAT.FMMaster
                                  where FMMaster.Date >= GetFmFrom.Value && FMMaster.FmNo.ToString().Trim() == mModel.SearchFMNo.Trim() && FMMaster.VehicleStatus != "100001"
                                  orderby FMMaster.FmNo
                                  select new FMMasterTrip()
                                  {
                                      RefTablekey = FMMaster.TableKey,
                                      FMNo = FMMaster.FmNo.ToString(),
                                      Date = FMMaster.Date,
                                      VehicleNo = FMMaster.VehicleStatus == "100001" ? ctxTFAT.HireVehicleMaster.Where(x => x.Code == FMMaster.TruckNo).Select(x => x.TruckNo).FirstOrDefault() : ctxTFAT.VehicleMaster.Where(x => x.Code == FMMaster.TruckNo).Select(x => x.TruckNo).FirstOrDefault(),
                                      Driver = ctxTFAT.DriverMaster.Where(x => x.Code == FMMaster.Driver).Select(x => x.Name).FirstOrDefault(),
                                      DriverC = FMMaster.Driver,
                                      From = ctxTFAT.TfatBranch.Where(x => x.Code == FMMaster.FromBranch).Select(x => x.Name).FirstOrDefault(),
                                      FromC = FMMaster.FromBranch,
                                      To = ctxTFAT.TfatBranch.Where(x => x.Code == FMMaster.ToBranch).Select(x => x.Name).FirstOrDefault(),
                                      ToC = FMMaster.ToBranch,
                                      RouteVia = FMMaster.RouteViaName,
                                      RouteViaC = FMMaster.RouteVia,
                                      Tripchages = 0,
                                      LocalCharges = 0,
                                      ViaCharges = 0,
                                      PaymentAmt = 0,
                                      Total = 0,
                                      DieselLtr = String.IsNullOrEmpty(FMMaster.DieselLtr) == true ? "0" : FMMaster.DieselLtr,
                                  }).ToList();
            }
            else
            {
                AllStockFMLIst = (from FMMaster in ctxTFAT.FMMaster
                                  where FMMaster.FmNo.ToString().Trim() == mModel.SearchFMNo.Trim() && FMMaster.VehicleStatus != "100001"
                                  orderby FMMaster.FmNo
                                  select new FMMasterTrip()
                                  {
                                      RefTablekey = FMMaster.TableKey,
                                      FMNo = FMMaster.FmNo.ToString(),
                                      Date = FMMaster.Date,
                                      VehicleNo = FMMaster.VehicleStatus == "100001" ? ctxTFAT.HireVehicleMaster.Where(x => x.Code == FMMaster.TruckNo).Select(x => x.TruckNo).FirstOrDefault() : ctxTFAT.VehicleMaster.Where(x => x.Code == FMMaster.TruckNo).Select(x => x.TruckNo).FirstOrDefault(),
                                      Driver = ctxTFAT.DriverMaster.Where(x => x.Code == FMMaster.Driver).Select(x => x.Name).FirstOrDefault(),
                                      DriverC = FMMaster.Driver,
                                      From = ctxTFAT.TfatBranch.Where(x => x.Code == FMMaster.FromBranch).Select(x => x.Name).FirstOrDefault(),
                                      FromC = FMMaster.FromBranch,
                                      To = ctxTFAT.TfatBranch.Where(x => x.Code == FMMaster.ToBranch).Select(x => x.Name).FirstOrDefault(),
                                      ToC = FMMaster.ToBranch,
                                      RouteVia = FMMaster.RouteViaName,
                                      RouteViaC = FMMaster.RouteVia,
                                      Tripchages = 0,
                                      LocalCharges = 0,
                                      ViaCharges = 0,
                                      PaymentAmt = 0,
                                      Total = 0,
                                      DieselLtr = String.IsNullOrEmpty(FMMaster.DieselLtr) == true ? "0" : FMMaster.DieselLtr,
                                  }).ToList();
            }

            List<FMMasterTrip> SelectedFm = TempData.Peek("SelecedFM") as List<FMMasterTrip>;
            if (SelectedFm == null)
            {
                SelectedFm = new List<FMMasterTrip>();
            }
            var FetSelectedFm = SelectedFm.Select(x => x.RefTablekey).ToList();
            AllStockFMLIst = AllStockFMLIst.Where(x => !(FetSelectedFm.Contains(x.RefTablekey))).ToList();
            var CreatedTripFmList = ctxTFAT.TripFmList.Select(x => x.RefTablekey).ToList();
            AllStockFMLIst = AllStockFMLIst.Where(x => !(CreatedTripFmList.Contains(x.RefTablekey))).ToList();


            mModel.fMMasters = AllStockFMLIst;
            TempData["PendingFM"] = AllStockFMLIst;
            var html = ViewHelper.RenderPartialView(this, "FMListCombo", mModel);
            var jsonResult = Json(new { Html = html }, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue;
            return jsonResult;
        }
        public ActionResult SearchLRList(TripSheetVM mModel)//Fetch Pending LR Related To Driver
        {
            mModel.FMOrNOt = false;
            List<FMMasterTrip> AllStockFMLIst = new List<FMMasterTrip>();
            var GetFmFrom = ctxTFAT.TripSheetSetup.Select(x => x.FetchFmFrom).FirstOrDefault();
            if (GetFmFrom != null)
            {
                AllStockFMLIst = (from FMMaster in ctxTFAT.LRMaster
                                  where FMMaster.BookDate >= GetFmFrom.Value && FMMaster.LrNo.ToString().Trim() == mModel.SearchFMNo.Trim()
                                  orderby FMMaster.LrNo
                                  select new FMMasterTrip()
                                  {
                                      RefTablekey = FMMaster.TableKey,
                                      FMNo = FMMaster.LrNo.ToString(),
                                      Date = FMMaster.BookDate,
                                      VehicleNo = ctxTFAT.VehicleMaster.Where(x => x.Code == FMMaster.VehicleNo).Select(x => x.TruckNo).FirstOrDefault(),
                                      Driver = ctxTFAT.DriverMaster.Where(x => x.Code == FMMaster.Driver).Select(x => x.Name).FirstOrDefault(),
                                      DriverC = FMMaster.Driver,
                                      From = ctxTFAT.TfatBranch.Where(x => x.Code == FMMaster.Source).Select(x => x.Name).FirstOrDefault(),
                                      FromC = FMMaster.Source,
                                      To = ctxTFAT.TfatBranch.Where(x => x.Code == FMMaster.Dest).Select(x => x.Name).FirstOrDefault(),
                                      ToC = FMMaster.Dest,
                                      RouteVia = ctxTFAT.CustomerMaster.Where(x => x.Code == FMMaster.BillParty).Select(x => x.Name).FirstOrDefault(),
                                      RouteViaC = FMMaster.BillParty,
                                      Tripchages = 0,
                                      LocalCharges = 0,
                                      ViaCharges = 0,
                                      DieselLtr = String.IsNullOrEmpty(FMMaster.DieselLtr) == true ? "0" : FMMaster.DieselLtr,
                                      Total = 0
                                  }).ToList();
            }
            else
            {
                AllStockFMLIst = (from FMMaster in ctxTFAT.LRMaster
                                  where FMMaster.LrNo.ToString().Trim() == mModel.SearchFMNo.Trim()
                                  orderby FMMaster.LrNo
                                  select new FMMasterTrip()
                                  {
                                      RefTablekey = FMMaster.TableKey,
                                      FMNo = FMMaster.LrNo.ToString(),
                                      Date = FMMaster.BookDate,
                                      VehicleNo = ctxTFAT.VehicleMaster.Where(x => x.Code == FMMaster.VehicleNo).Select(x => x.TruckNo).FirstOrDefault(),
                                      Driver = ctxTFAT.DriverMaster.Where(x => x.Code == FMMaster.Driver).Select(x => x.Name).FirstOrDefault(),
                                      DriverC = FMMaster.Driver,
                                      From = ctxTFAT.TfatBranch.Where(x => x.Code == FMMaster.Source).Select(x => x.Name).FirstOrDefault(),
                                      FromC = FMMaster.Source,
                                      To = ctxTFAT.TfatBranch.Where(x => x.Code == FMMaster.Dest).Select(x => x.Name).FirstOrDefault(),
                                      ToC = FMMaster.Dest,
                                      RouteVia = ctxTFAT.CustomerMaster.Where(x => x.Code == FMMaster.BillParty).Select(x => x.Name).FirstOrDefault(),
                                      RouteViaC = FMMaster.BillParty,
                                      Tripchages = 0,
                                      LocalCharges = 0,
                                      ViaCharges = 0,
                                      DieselLtr = String.IsNullOrEmpty(FMMaster.DieselLtr) == true ? "0" : FMMaster.DieselLtr,
                                      Total = 0
                                  }).ToList();
            }

            List<FMMasterTrip> SelectedFm = TempData.Peek("SelecedLR") as List<FMMasterTrip>;
            if (SelectedFm == null)
            {
                SelectedFm = new List<FMMasterTrip>();
            }
            var FetSelectedFm = SelectedFm.Select(x => x.RefTablekey).ToList();
            AllStockFMLIst = AllStockFMLIst.Where(x => !(FetSelectedFm.Contains(x.RefTablekey))).ToList();
            var CreatedTripFmList = ctxTFAT.TripFmList.Select(x => x.RefTablekey).ToList();
            AllStockFMLIst = AllStockFMLIst.Where(x => !(CreatedTripFmList.Contains(x.RefTablekey))).ToList();


            mModel.fMMasters = AllStockFMLIst;
            TempData["PendingLR"] = AllStockFMLIst;
            var html = ViewHelper.RenderPartialView(this, "FMListCombo", mModel);
            var jsonResult = Json(new { Html = html }, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue;
            return jsonResult;
        }

        public ActionResult GetCCList(TripSheetVM mModel)//Fetch AdvancePaid To Driver
        {

            var StartFrom = ConvertDDMMYYTOYYMMDD(mModel.CCFromDate);
            var End = ConvertDDMMYYTOYYMMDD(mModel.CCTODate);
            List<Ledger> ledgers = new List<Ledger>();
            if (mModel.VehicleFlag)
            {
                var POstAc = ctxTFAT.VehicleMaster.Where(x => x.Code == mModel.VehicleCode).Select(x => x.PostAc).FirstOrDefault();
                if (!String.IsNullOrEmpty(POstAc))
                {

                    var ListLEgerRef = ctxTFAT.RelateData.Where(x => x.AmtType == true && POstAc.ToLower().Trim() == x.Value8.ToLower().Trim()).Select(x => x.ParentKey.Trim() + x.Branch.Trim() + x.Code.Trim()).ToList();
                    var GetLEdgetList = ctxTFAT.Ledger.Where(x => ListLEgerRef.Contains(x.ParentKey.Trim() + x.Branch.Trim() + x.Code.Trim()) && x.Debit != 0 && x.DocDate >= StartFrom && x.DocDate <= End && x.Type.ToLower() != "trip0").ToList();
                    foreach (var item in GetLEdgetList)
                    {
                        var RelaTeData = ctxTFAT.RelateData.Where(x => x.AmtType == true && POstAc.ToLower().Trim() == x.Value8.ToLower().Trim() && x.ParentKey + x.Branch == item.ParentKey + item.Branch && (x.Combo1 == "000100343" || x.Code == "000100343")).FirstOrDefault();
                        var allkey = ctxTFAT.TripSheetMaster.Where(x => x.DocNo != mModel.Document).Select(x => x.CCjustLedgerRef).ToList();
                        List<string> List = new List<string>();
                        foreach (var key in allkey)
                        {
                            List.AddRange(key.Split('^'));
                        }
                        var Check = List.Where(x => x == item.Branch + item.TableKey).FirstOrDefault();
                        if (Check == null)
                        {
                            ledgers.Add(new Ledger
                            {
                                DocDate = item.DocDate,
                                Debit = item.Debit,
                                Credit = item.Credit,
                                AltCode = ctxTFAT.Master.Where(x => x.Code == item.AltCode).Select(x => x.Name).FirstOrDefault(),
                                Code = ctxTFAT.Master.Where(x => x.Code == item.Code).Select(x => x.Name).FirstOrDefault(),
                                Narr = item.Narr,
                                TableKey = item.TableKey,
                                Branch = item.Branch,
                                Party = ctxTFAT.TfatBranch.Where(x => x.Code == item.Branch).Select(x => x.Name).FirstOrDefault(),
                                RefDoc = RelaTeData == null ? "0" : (String.IsNullOrEmpty(RelaTeData.Value2) == true ? "0" : RelaTeData.Value2),
                                AUTHORISE = RelaTeData == null ? "" : (String.IsNullOrEmpty(RelaTeData.Value4) == true ? "" : RelaTeData.Value4),
                                AUTHIDS = RelaTeData == null ? "0" : (String.IsNullOrEmpty(RelaTeData.Value3) == true ? "0" : RelaTeData.Value3),
                            });
                        }

                    }
                }
            }
            else
            {
                var POstAc = ctxTFAT.DriverMaster.Where(x => x.Code == mModel.DriverCode).Select(x => x.Posting).FirstOrDefault();
                if (!String.IsNullOrEmpty(POstAc))
                {

                    var ListLEgerRef = ctxTFAT.RelateData.Where(x => x.AmtType == true && POstAc.ToLower().Trim() == x.Value8.ToLower().Trim()).Select(x => x.ParentKey.Trim() + x.Branch.Trim() + x.Code.Trim()).ToList();
                    var GetLEdgetList = ctxTFAT.Ledger.Where(x => ListLEgerRef.Contains(x.ParentKey.Trim() + x.Branch.Trim() + x.Code.Trim()) && x.Debit != 0 && x.DocDate >= StartFrom && x.DocDate <= End && x.Type.ToLower() != "trip0").ToList();
                    foreach (var item in GetLEdgetList)
                    {
                        var RelaTeData = ctxTFAT.RelateData.Where(x => x.AmtType == true && POstAc.ToLower().Trim() == x.Value8.ToLower().Trim() && x.ParentKey + x.Branch == item.ParentKey + item.Branch && (x.Combo1 == "000100343" || x.Code == "000100343")).FirstOrDefault();
                        var allkey = ctxTFAT.TripSheetMaster.Where(x => x.DocNo != mModel.Document).Select(x => x.CCjustLedgerRef).ToList();
                        List<string> List = new List<string>();
                        foreach (var key in allkey)
                        {
                            List.AddRange(key.Split('^'));
                        }
                        var Check = List.Where(x => x == item.Branch + item.TableKey).FirstOrDefault();
                        if (Check == null)
                        {
                            ledgers.Add(new Ledger
                            {
                                DocDate = item.DocDate,
                                Debit = item.Debit,
                                Credit = item.Credit,
                                AltCode = ctxTFAT.Master.Where(x => x.Code == item.AltCode).Select(x => x.Name).FirstOrDefault(),
                                Code = ctxTFAT.Master.Where(x => x.Code == item.Code).Select(x => x.Name).FirstOrDefault(),
                                Narr = item.Narr,
                                TableKey = item.TableKey,
                                Branch = item.Branch,
                                Party = ctxTFAT.TfatBranch.Where(x => x.Code == item.Branch).Select(x => x.Name).FirstOrDefault(),
                                RefDoc = RelaTeData == null ? "0" : (String.IsNullOrEmpty(RelaTeData.Value2) == true ? "0" : RelaTeData.Value2),
                                AUTHORISE = RelaTeData == null ? "" : (String.IsNullOrEmpty(RelaTeData.Value4) == true ? "" : RelaTeData.Value4),
                                AUTHIDS = RelaTeData == null ? "0" : (String.IsNullOrEmpty(RelaTeData.Value3) == true ? "0" : RelaTeData.Value3),
                            });
                        }

                    }
                }
            }



            if (mModel.CCledgers != null)
            {
                var BRANCHTapelkey = mModel.CCledgers.Select(x => x.Branch + x.TableKey).ToList();
                foreach (var item in ledgers)
                {
                    if (BRANCHTapelkey.Contains(item.Branch + item.TableKey))
                    {
                        item.TDSFlag = true;
                    }
                }
            }

            mModel.CCledgers = ledgers;


            mModel.CCledgers = ledgers;
            TempData["CCLedgerList"] = ledgers;
            var html = ViewHelper.RenderPartialView(this, "CCLedgerPaidSummary", mModel);
            var jsonResult = Json(new { Html = html }, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue;
            return jsonResult;
        }
        bool ExistsInKeys(List<string> keys, string branch, string tableKey)
        {
            return keys.Any(key => key == branch + tableKey);
        }

        public ActionResult AddFMToList(TripSheetVM mModel)//Selected Fm Show In Grid
        {

            if (mModel.FMOrNOt)
            {
                TempData.Remove("PendingLR");
                TempData.Remove("SelecedLR");
            }
            else
            {
                TempData.Remove("PendingFM");
                TempData.Remove("SelecedFM");
            }
            List<FMMasterTrip> PendingFM = TempData.Peek("PendingFM") as List<FMMasterTrip>;
            List<FMMasterTrip> SelectedFm = TempData.Peek("SelecedFM") as List<FMMasterTrip>;
            if (SelectedFm == null)
            {
                SelectedFm = new List<FMMasterTrip>();
            }
            List<FMMasterTrip> PendingLR = TempData.Peek("PendingLR") as List<FMMasterTrip>;
            List<FMMasterTrip> SelectedLR = TempData.Peek("SelecedLR") as List<FMMasterTrip>;
            if (SelectedLR == null)
            {
                SelectedLR = new List<FMMasterTrip>();
            }
            if (mModel.FMOrNOt)
            {
                SelectedFm.AddRange(mModel.fMMasters);
                TempData["SelecedFM"] = SelectedFm;
                mModel.fMMasters = SelectedFm;
                //mModel.Narr = "FMNo : ";
                //foreach (var item in SelectedFm.Select(x => x.FMNo).Distinct().ToList())
                //{
                //    mModel.Narr += item + ",";
                //}
                //mModel.Narr = mModel.Narr.Substring(0, mModel.Narr.Length - 1);
                //mModel.Narr += "      VehicleNo : ";
                //foreach (var item in SelectedFm.Select(x => x.VehicleNo).Distinct().ToList())
                //{
                //    mModel.Narr += item + ",";
                //}
                //mModel.Narr = mModel.Narr.Substring(0, mModel.Narr.Length - 1);

                foreach (var item in SelectedFm)
                {
                    var list = ctxTFAT.FMROUTETable.Where(x => x.Parentkey.ToString().Trim() == item.RefTablekey.Trim() && string.IsNullOrEmpty(x.Narr) == false).ToList().Count;
                    if (list > 0)
                    {
                        item.NarrReq = true;
                    }

                    #region Check Schedule There Or NOt
                    decimal StartKM = 0, EndKM = 0;
                    bool ShowSchedule = false;
                    #region Start KM OF Vehicle
                    var FirstRoute = ctxTFAT.FMROUTETable.Where(x => x.Parentkey.ToString() == item.RefTablekey.ToString() && x.SequenceRoute == 0).ToList();
                    foreach (var route in FirstRoute)
                    {
                        if (route.ArrivalKM != null || route.DispatchKM != null)
                        {
                            if (route.ArrivalKM != null)
                            {
                                StartKM = route.ArrivalKM.Value;
                            }
                            else
                            {
                                StartKM = route.DispatchKM.Value;
                            }
                            break;
                        }
                    }

                    var TotalRoute = ctxTFAT.FMROUTETable.Where(x => x.Parentkey.ToString() == item.RefTablekey.ToString() && x.RouteType == "R").ToList().Count() - 1;
                    var LastRoute = ctxTFAT.FMROUTETable.Where(x => x.Parentkey.ToString() == item.RefTablekey.ToString() && x.SubRoute == TotalRoute).OrderByDescending(x => x.RECORDKEY).FirstOrDefault();
                    if (LastRoute.ArrivalReSchKm != null)
                    {
                        EndKM = LastRoute.ArrivalReSchKm.Value;
                    }

                    var Count = ctxTFAT.FMROUTETable.Where(x => x.Parentkey.ToString() == item.RefTablekey.ToString() && String.IsNullOrEmpty(x.ArrivalSchTime) == false).ToList().Count();
                    if (Count > 0)
                    {
                        ShowSchedule = true;
                    }
                    item.ShowSchedule = ShowSchedule;
                    item.StartKM = StartKM;
                    item.EndKM = EndKM;
                    item.RunningKM = EndKM - StartKM;
                    #endregion
                    #endregion
                }
            }
            else
            {
                SelectedLR.AddRange(mModel.fMMasters);
                TempData["SelecedLR"] = SelectedLR;
                mModel.fMMasters = SelectedLR;
                mModel.Narr = "LRNo : ";
                foreach (var item in SelectedLR.Select(x => x.FMNo).Distinct().ToList())
                {
                    mModel.Narr += item + ",";
                }
                mModel.Narr = mModel.Narr.Substring(0, mModel.Narr.Length - 1);

                mModel.Narr += "      VehicleNo : ";
                foreach (var item in SelectedLR.Select(x => x.VehicleNo).Distinct().ToList())
                {
                    mModel.Narr += item + ",";
                }
                mModel.Narr = mModel.Narr.Substring(0, mModel.Narr.Length - 1);
            }

            var html = ViewHelper.RenderPartialView(this, "DocumentList", mModel);
            return Json(new { Html = html, Narr = mModel.Narr }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult DeleteSelecedFM(string FMNO)//Delete Fm From Grid
        {
            List<FMMasterTrip> SelectedFm = TempData.Peek("SelecedFM") as List<FMMasterTrip>;
            if (SelectedFm == null)
            {
                SelectedFm = new List<FMMasterTrip>();
            }
            SelectedFm = SelectedFm.Where(x => x.RefTablekey != FMNO).ToList();
            TempData["SelecedFM"] = SelectedFm;

            return Json(new { Status = "Success" }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult ChangeDriver(TripSheetVM mModel)//If Driver Change Then Refresh The Grid
        {
            TempData.Remove("PendingFM");
            TempData.Remove("SelecedFM");
            TempData.Remove("PendingLR");
            TempData.Remove("SelecedLR");
            TempData.Remove("CCLedgerList");
            TempData.Remove("LedgerList");

            List<FMMasterTrip> SelectedFm = new List<FMMasterTrip>();

            mModel.fMMasters = SelectedFm;

            mModel.Advledgers = new List<Ledger>();
            mModel.CCledgers = new List<Ledger>();

            var html = ViewHelper.RenderPartialView(this, "DocumentList", mModel);
            var Advhtml = ViewHelper.RenderPartialView(this, "SaveLedgerPaidSummary", mModel);
            var CChtml = ViewHelper.RenderPartialView(this, "CCLedgerPaidSummary", mModel);

            string SpclRemark = "", BlackListRemark = "", TDSName = "";
            bool HireSpcl = false, HireBlackList = false, CutTDS = false;
            int TDSCode = 0;
            decimal TDSRate = 0;
            double mBalance = 0;
            if (mModel.VehicleFlag)
            {
                VehicleMaster consigner = ctxTFAT.VehicleMaster.Where(x => x.Code == mModel.VehicleCode).FirstOrDefault();
                if (consigner != null)
                {

                    if (!String.IsNullOrEmpty(consigner.Remark))
                    {
                        HireSpcl = true;
                        SpclRemark = consigner.Remark;
                    }
                    if (!String.IsNullOrEmpty(consigner.HoldRemark))
                    {
                        HireBlackList = true;
                        BlackListRemark = consigner.HoldRemark;
                    }
                }
                else
                {
                    consigner = new VehicleMaster();
                }

                string mStr = @"select dbo.GetBalance('" + consigner.PostAc + "','" + MMDDYY(DateTime.Now) + "','',0,0)";
                DataTable smDt = GetDataTable(mStr);

                if (smDt.Rows.Count > 0)
                {
                    mBalance = Convert.ToDouble(smDt.Rows[0][0].ToString());
                }

                var taxdetails = ctxTFAT.TaxDetails.Where(x => x.Code == consigner.PostAc).Select(x => new { x.TDSCode, x.CutTDS, x.TDSRate }).FirstOrDefault();
                CutTDS = taxdetails == null ? false : taxdetails.CutTDS;
                TDSCode = taxdetails == null ? 0 : taxdetails.TDSCode == null ? 0 : taxdetails.TDSCode.Value;
                //TDSRate = taxdetails == null ? 0 : taxdetails.TDSRate == null ? 0 : taxdetails.TDSRate.Value;
                var CurrDate = ConvertDDMMYYTOYYMMDD(mModel.TripSheetDate);
                TDSRate = ctxTFAT.TDSRates.Where(x => x.Code == TDSCode && x.EffDate <= CurrDate).OrderByDescending(x => x.EffDate).Select(x => x.TDSRate ?? 0).FirstOrDefault();

                TDSName = ctxTFAT.TDSMaster.Where(x => x.Code == TDSCode).Select(x => x.Name).FirstOrDefault();
            }
            else
            {
                DriverMaster consigner = ctxTFAT.DriverMaster.Where(x => x.Code == mModel.DriverCode).FirstOrDefault();
                if (consigner != null)
                {

                    if (!String.IsNullOrEmpty(consigner.Ticklers))
                    {
                        HireSpcl = true;
                        SpclRemark = consigner.Ticklers;
                    }
                    if (!String.IsNullOrEmpty(consigner.HoldTicklers))
                    {
                        HireBlackList = true;
                        BlackListRemark = consigner.HoldTicklers;
                    }
                }



                mModel.DriverCode = ctxTFAT.DriverMaster.Where(x => x.Code == mModel.DriverCode).Select(x => x.Posting).FirstOrDefault();

                string mStr = @"select dbo.GetBalance('" + mModel.DriverCode + "','" + MMDDYY(DateTime.Now) + "','',0,0)";
                DataTable smDt = GetDataTable(mStr);
                if (smDt.Rows.Count > 0)
                {
                    mBalance = Convert.ToDouble(smDt.Rows[0][0].ToString());
                }

                var taxdetails = ctxTFAT.TaxDetails.Where(x => x.Code == mModel.DriverCode).Select(x => new { x.TDSCode, x.CutTDS, x.TDSRate }).FirstOrDefault();
                CutTDS = taxdetails == null ? false : taxdetails.CutTDS;
                TDSCode = taxdetails == null ? 0 : taxdetails.TDSCode == null ? 0 : taxdetails.TDSCode.Value;
                //TDSRate = taxdetails == null ? 0 : taxdetails.TDSRate == null ? 0 : taxdetails.TDSRate.Value;
                var CurrDate = ConvertDDMMYYTOYYMMDD(mModel.TripSheetDate);
                TDSRate = ctxTFAT.TDSRates.Where(x => x.Code == TDSCode && x.EffDate <= CurrDate).OrderByDescending(x => x.EffDate).Select(x => x.TDSRate ?? 0).FirstOrDefault();
                TDSName = ctxTFAT.TDSMaster.Where(x => x.Code == TDSCode).Select(x => x.Name).FirstOrDefault();
            }

            return Json(new
            {
                Html = html,
                CutTDS = CutTDS,
                TDSCode = TDSCode,
                TDSRate = TDSRate,
                TDSName = TDSName,
                Advhtml = Advhtml,
                CChtml = CChtml,
                Balance = mBalance,
                HireSpcl = HireSpcl,
                HireSpclRemark = SpclRemark,
                HireBlackList = HireBlackList,
                HireBlackListRemark = BlackListRemark,

            }, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region Ledger Advance

        public ActionResult GetAdvanceList(TripSheetVM mModel)//Fetch AdvancePaid To Driver
        {
            if (mModel.Advledgers == null)
            {
                mModel.Advledgers = new List<Ledger>();
            }
            var StartFrom = ConvertDDMMYYTOYYMMDD(mModel.AdvFromDate);
            var End = ConvertDDMMYYTOYYMMDD(mModel.AdvTODate);
            List<Ledger> ledgers = new List<Ledger>();
            if (!mModel.Pick_Financial_Document)
            {
                StartFrom = ConvertDDMMYYTOYYMMDD("01/01/2000");
                End = ConvertDDMMYYTOYYMMDD(DateTime.Now.ToShortDateString());
            }
            if (mModel.VehicleFlag)
            {
                var POstAc = ctxTFAT.VehicleMaster.Where(x => x.Code == mModel.VehicleCode).Select(x => x.PostAc).FirstOrDefault();
                if (!String.IsNullOrEmpty(POstAc))
                {

                    var tripSheetData = ctxTFAT.TripSheetMaster
                        .Where(x => x.DocNo != mModel.Document)
                        .ToList();

                    var allKeys = tripSheetData
                        .SelectMany(x => x.AdjustLedgerRef.Split('^'));

                    var Keylist = allKeys.ToList();
                    var list = ctxTFAT.Ledger
                        .Where(x => x.Code == POstAc && x.Debit != 0 && x.DocDate >= StartFrom && x.DocDate <= End && x.Type.ToLower() != "trip0")
                        .AsEnumerable()
                        .Where(item => !ExistsInKeys(Keylist, item.Branch, item.TableKey))
                        .Select(item => new Ledger
                        {
                            DocDate = item.DocDate,
                            Debit = item.Debit,
                            Credit = item.Credit,
                            Code = ctxTFAT.Master.Where(x => x.Code == item.Code).Select(x => x.Name).FirstOrDefault(),
                            AltCode = ctxTFAT.Master.Where(x => x.Code == item.AltCode).Select(x => x.Name).FirstOrDefault(),
                            TableKey = item.TableKey,
                            Branch = item.Branch,
                            Party = ctxTFAT.TfatBranch.Where(x => x.Code == item.Branch).Select(x => x.Name).FirstOrDefault(),
                            Narr = item.Narr,
                            Srl = item.Srl
                        })
                        .ToList();

                    var SelectedAdv = mModel.Advledgers.Select(x => x.Branch + x.TableKey).ToList();
                    list = list.Where(x => SelectedAdv.Any(y => y != x.Branch + x.TableKey)).ToList();
                    ledgers.AddRange(list);


                }
            }
            else
            {
                var POstAc = ctxTFAT.DriverMaster.Where(x => x.Code == mModel.DriverCode).Select(x => x.Posting).FirstOrDefault();
                if (!String.IsNullOrEmpty(POstAc))
                {
                    var tripSheetData = ctxTFAT.TripSheetMaster
                        .Where(x => x.DocNo != mModel.Document)
                        .ToList();

                    var allKeys = tripSheetData
                        .SelectMany(x => x.AdjustLedgerRef.Split('^'));

                    var Keylist = allKeys.ToList();
                    var list = ctxTFAT.Ledger
                        .Where(x => x.Code == POstAc && x.Debit != 0 && StartFrom <= x.DocDate && x.DocDate <= End && x.Type.ToLower() != "trip0")
                        .AsEnumerable()
                        .Where(item => !ExistsInKeys(Keylist, item.Branch, item.TableKey))
                        .Select(item => new Ledger
                        {
                            DocDate = item.DocDate,
                            Debit = item.Debit,
                            Credit = item.Credit,
                            Code = ctxTFAT.Master.Where(x => x.Code == item.Code).Select(x => x.Name).FirstOrDefault(),
                            AltCode = ctxTFAT.Master.Where(x => x.Code == item.AltCode).Select(x => x.Name).FirstOrDefault(),
                            TableKey = item.TableKey,
                            Branch = item.Branch,
                            Party = ctxTFAT.TfatBranch.Where(x => x.Code == item.Branch).Select(x => x.Name).FirstOrDefault(),
                            Narr = item.Narr,
                            Srl = item.Srl
                        })
                        .ToList();
                    var SelectedAdv = mModel.Advledgers.Select(x => x.Branch + x.TableKey).ToList();
                    list = list.Where(x => !SelectedAdv.Any(y => y == x.Branch + x.TableKey)).ToList();
                    ledgers.AddRange(list);
                }
            }

            //if (mModel.Advledgers != null)
            //{
            //    var BRANCHTapelkey = mModel.Advledgers.Select(x => x.Branch + x.TableKey).ToList();
            //    foreach (var item in ledgers)
            //    {
            //        if (BRANCHTapelkey.Contains(item.Branch + item.TableKey))
            //        {
            //            item.TDSFlag = true;
            //        }
            //    }
            //}

            mModel.Advledgers = ledgers;
            //TempData["LedgerList"] = ledgers;
            var html = ViewHelper.RenderPartialView(this, "LedgerPaidSummary", mModel);
            var jsonResult = Json(new { Html = html }, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue;
            return jsonResult;
        }

        public ActionResult SaveAdvanceList(TripSheetVM mModel)//Fetch AdvancePaid To Driver
        {
            var existAdv = TempData.Peek("LedgerList") as List<Ledger>;
            if (existAdv == null)
            {
                existAdv = new List<Ledger>();
            }
            existAdv.AddRange(mModel.Advledgers);
            TempData["LedgerList"] = existAdv;
            mModel.Advledgers = existAdv;
            var html = ViewHelper.RenderPartialView(this, "SaveLedgerPaidSummary", mModel);
            var jsonResult = Json(new { Html = html }, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue;
            return jsonResult;
        }

        public ActionResult DeleteAdv(TripSheetVM mModel)
        {
            var existAdv = TempData.Peek("LedgerList") as List<Ledger>;
            if (existAdv == null)
            {
                existAdv = new List<Ledger>();
            }
            existAdv = existAdv.Where(x => x.Branch + x.TableKey != mModel.ledger.Branch + mModel.ledger.TableKey).ToList();
            TempData["LedgerList"] = existAdv;
            var jsonResult = Json(new { Status = "Sucess" }, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue;
            return jsonResult;
        }

        #endregion

        #region Ledger Balance

        public ActionResult GetBalList(TripSheetVM mModel)//Fetch AdvancePaid To Driver
        {
            if (mModel.Balledgers == null)
            {
                mModel.Balledgers = new List<Ledger>();
            }
            var StartFrom = ConvertDDMMYYTOYYMMDD(mModel.BalFromDate);
            var End = ConvertDDMMYYTOYYMMDD(mModel.BalTODate);
            List<Ledger> ledgers = new List<Ledger>();
            if (!mModel.Pick_Financial_Document)
            {
                StartFrom = ConvertDDMMYYTOYYMMDD("01/01/2000");
                End = ConvertDDMMYYTOYYMMDD(DateTime.Now.ToShortDateString());
            }
            if (mModel.VehicleFlag)
            {
                var POstAc = ctxTFAT.VehicleMaster.Where(x => x.Code == mModel.VehicleCode).Select(x => x.PostAc).FirstOrDefault();
                if (!String.IsNullOrEmpty(POstAc))
                {
                    var tripSheetData = ctxTFAT.TripSheetMaster
                        .Where(x => x.DocNo != mModel.Document)
                        .ToList();

                    var allKeys = tripSheetData
                        .SelectMany(x => x.AdjustBalLedgerRef.Split('^'));

                    var Keylist = allKeys.ToList();
                    var list = ctxTFAT.Ledger
                        .Where(x => x.Code == POstAc && x.Credit != 0 && x.DocDate >= StartFrom && x.DocDate <= End && x.Type.ToLower() != "trip0")
                        .AsEnumerable()
                        .Where(item => !ExistsInKeys(Keylist, item.Branch, item.TableKey))
                        .Select(item => new Ledger
                        {
                            Srl = item.Srl,
                            DocDate = item.DocDate,
                            Debit = item.Debit,
                            Credit = item.Credit,
                            Code = ctxTFAT.Master.Where(x => x.Code == item.Code).Select(x => x.Name).FirstOrDefault(),
                            AltCode = ctxTFAT.Master.Where(x => x.Code == item.AltCode).Select(x => x.Name).FirstOrDefault(),
                            TableKey = item.TableKey,
                            Branch = item.Branch,
                            Party = ctxTFAT.TfatBranch.Where(x => x.Code == item.Branch).Select(x => x.Name).FirstOrDefault(),
                            Narr = item.Narr
                        })
                        .ToList();

                    var SelectedBal = mModel.Balledgers.Select(x => x.Branch + x.TableKey).ToList();
                    list = list.Where(x => !SelectedBal.Any(y => y == x.Branch + x.TableKey)).ToList();
                    ledgers.AddRange(list);
                }
            }
            else
            {
                var POstAc = ctxTFAT.DriverMaster.Where(x => x.Code == mModel.DriverCode).Select(x => x.Posting).FirstOrDefault();
                if (!String.IsNullOrEmpty(POstAc))
                {
                    var tripSheetData = ctxTFAT.TripSheetMaster
                        .Where(x => x.DocNo != mModel.Document && x.AdjustBalLedgerRef != null)
                        .ToList();

                    var allKeys = tripSheetData
                        .SelectMany(x => x.AdjustBalLedgerRef.Split('^'));

                    var Keylist = allKeys.ToList();
                    var list = ctxTFAT.Ledger
                        .Where(x => x.Code == POstAc && x.Credit != 0 && x.DocDate >= StartFrom && x.DocDate <= End && x.Type.ToLower() != "trip0")
                        .AsEnumerable()
                        .Where(item => !ExistsInKeys(Keylist, item.Branch, item.TableKey))
                        .Select(item => new Ledger
                        {
                            Srl = item.Srl,
                            DocDate = item.DocDate,
                            Debit = item.Debit,
                            Credit = item.Credit,
                            Code = ctxTFAT.Master.Where(x => x.Code == item.Code).Select(x => x.Name).FirstOrDefault(),
                            AltCode = ctxTFAT.Master.Where(x => x.Code == item.AltCode).Select(x => x.Name).FirstOrDefault(),
                            TableKey = item.TableKey,
                            Branch = item.Branch,
                            Party = ctxTFAT.TfatBranch.Where(x => x.Code == item.Branch).Select(x => x.Name).FirstOrDefault(),
                            Narr = item.Narr
                        })
                        .ToList();

                    var SelectedBal = mModel.Balledgers.Select(x => x.Branch + x.TableKey).ToList();
                    list = list.Where(x => !SelectedBal.Any(y => y == x.Branch + x.TableKey)).ToList();
                    ledgers.AddRange(list);
                }
            }

            //if (mModel.Balledgers != null)
            //{
            //    var BRANCHTapelkey = mModel.Balledgers.Select(x => x.Branch + x.TableKey).ToList();
            //    foreach (var item in ledgers)
            //    {
            //        if (BRANCHTapelkey.Contains(item.Branch + item.TableKey))
            //        {
            //            item.TDSFlag = true;
            //        }
            //    }
            //}

            mModel.Balledgers = ledgers;

            //TempData["BalLedgerList"] = ledgers;
            var html = ViewHelper.RenderPartialView(this, "LedgerBalSummary", mModel);
            var jsonResult = Json(new { Html = html }, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue;
            return jsonResult;
        }

        public ActionResult SaveBalanceList(TripSheetVM mModel)//Fetch AdvancePaid To Driver
        {
            var existAdv = TempData.Peek("BalLedgerList") as List<Ledger>;
            if (existAdv == null)
            {
                existAdv = new List<Ledger>();
            }
            existAdv.AddRange(mModel.Advledgers);
            TempData["BalLedgerList"] = existAdv;
            mModel.Balledgers = existAdv;
            var html = ViewHelper.RenderPartialView(this, "SaveLedgerBalSummary", mModel);
            var jsonResult = Json(new { Html = html }, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue;
            return jsonResult;
        }

        public ActionResult DeleteBal(TripSheetVM mModel)
        {
            var existAdv = TempData.Peek("BalLedgerList") as List<Ledger>;
            if (existAdv == null)
            {
                existAdv = new List<Ledger>();
            }
            existAdv = existAdv.Where(x => x.Branch + x.TableKey != mModel.ledger.Branch + mModel.ledger.TableKey).ToList();
            TempData["BalLedgerList"] = existAdv;
            var jsonResult = Json(new { Status = "Sucess" }, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue;
            return jsonResult;
        }

        #endregion


        #region Second Tab(Other Expenses)

        public ActionResult GetBreakExpenses(TripSheetVM Model)
        {
            if (Model.Mode == "Edit")
            {
                var result = (List<OtherExpenses>)Session["OtherExpensesList"];
                var result1 = result.Where(x => x.tempId == Model.otherExpenses.tempId);
                foreach (var item in result1)
                {
                    var Partial = "";
                    if (item.LRDetailList != null && item.LRDetailList.Count() > 0)
                    {
                        Partial = "LR";
                    }
                    if (item.FMDetailList != null && item.FMDetailList.Count() > 0)
                    {
                        Partial = "FM";
                    }
                    if (String.IsNullOrEmpty(Partial))
                    {
                        Partial = ctxTFAT.Master.Where(x => x.Code == item.ExpensesAc).Select(x => x.RelatedTo).FirstOrDefault();
                    }
                    Model.otherExpenses.ExpensesAc = item.ExpensesAc;
                    Model.otherExpenses.ExpensesAcName = item.ExpensesAcName;
                    Model.otherExpenses.DocNo = item.DocNo;
                    Model.otherExpenses.Amount = item.Amount;
                    Model.otherExpenses.Narr = item.Narr;
                    Model.otherExpenses.tempId = item.tempId;
                    Model.otherExpenses.RelatedTo = item.RelatedTo;
                    Model.PartialDivName = Partial;
                    Model.CostCenterTally = item.CostCenterTally;
                    Model.LRDetailList = item.LRDetailList;
                    Model.FMDetailList = item.FMDetailList;
                }
                var jsonResult = Json(new { Html = this.RenderPartialView("AddNewExpenses", Model) }, JsonRequestBehavior.AllowGet);
                jsonResult.MaxJsonLength = int.MaxValue;
                return jsonResult;
            }
            else
            {
                Model.otherExpenses = new OtherExpenses();
                //var GetExpensesAccount = ctxTFAT.Master.Where(x => x.Code == Model.OtherExpAc).FirstOrDefault();
                //Model.otherExpenses.ExpensesAc = GetExpensesAccount.Code;
                //Model.otherExpenses.ExpensesAcName = GetExpensesAccount.Name;
                Model.otherExpenses.Amount = 0;



                var jsonResult = Json(new { Html = this.RenderPartialView("AddNewExpenses", Model) }, JsonRequestBehavior.AllowGet);
                jsonResult.MaxJsonLength = int.MaxValue;
                return jsonResult;
            }
        }

        [HttpPost]
        public ActionResult AddEditSelectedExpenses(TripSheetVM Model)
        {
            if (Model.Mode == "Add")
            {
                List<OtherExpenses> objledgerdetail = new List<OtherExpenses>();
                if (Session["OtherExpensesList"] != null)
                {
                    objledgerdetail = (List<OtherExpenses>)Session["OtherExpensesList"];
                }

                Model.otherExpenses.tempId = objledgerdetail.Count() + 1;
                objledgerdetail.Add(Model.otherExpenses);

                Session.Add("OtherExpensesList", objledgerdetail);
                Model.expenseslist = objledgerdetail;
                var html = ViewHelper.RenderPartialView(this, "OtherExpensesList", Model);
                return Json(new { Html = html }, JsonRequestBehavior.AllowGet);
            }
            else

            {
                var result = (List<OtherExpenses>)Session["OtherExpensesList"];

                var OtherExpenses = result.Where(x => x.tempId == Model.otherExpenses.tempId).FirstOrDefault();

                OtherExpenses.ExpensesAc = Model.otherExpenses.ExpensesAc;
                OtherExpenses.ExpensesAcName = Model.otherExpenses.ExpensesAcName;
                OtherExpenses.DocNo = Model.otherExpenses.DocNo == null ? "" : Model.otherExpenses.DocNo;
                OtherExpenses.Amount = Model.otherExpenses.Amount;
                OtherExpenses.LRDetailList = Model.otherExpenses.LRDetailList;
                OtherExpenses.FMDetailList = Model.otherExpenses.FMDetailList;
                OtherExpenses.RelatedTo = Model.otherExpenses.RelatedTo;
                OtherExpenses.CostCenterTally = Model.CostCenterTally;
                OtherExpenses.Narr = Model.otherExpenses.Narr;
                Model.expenseslist = result;

                var html = ViewHelper.RenderPartialView(this, "OtherExpensesList", Model);
                return Json(new
                {
                    Html = html
                }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult DeleteExpenses(TripSheetVM Model)
        {
            var result2 = (List<OtherExpenses>)Session["OtherExpensesList"];
            var result = result2.Where(x => x.tempId != Model.otherExpenses.tempId).ToList();
            int i = 1;
            foreach (var item in result)
            {
                item.tempId = i++;
            }
            Session.Add("OtherExpensesList", result);
            Model.expenseslist = result;
            var html = ViewHelper.RenderPartialView(this, "OtherExpensesList", Model);
            return Json(new { Html = html }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult FetExpensesRetedDocType(TripSheetVM Model)
        {
            string PartialDivName = "", html = "";
            var mAcc = ctxTFAT.Master.Where(x => x.Code == Model.Code).FirstOrDefault();

            var mIsVehicle = ctxTFAT.Master.Where(x => x.Code == Model.Code).Select(x => x.OthPostType).FirstOrDefault();

            if (mAcc.RelatedTo == "LR")
            {
                PartialDivName = "LR";
            }
            else if (mAcc.RelatedTo == "FM")
            {
                PartialDivName = "FM";
            }
            else
            {
                PartialDivName = "N";
            }

            var ReferAccReq = mAcc.ReferAccReq;
            var CostCenterTally = mAcc.CostCenterAmtTally;
            var LRDetails = this.RenderPartialView("LRDetails", Model);
            var FMDetails = this.RenderPartialView("FMDetails", Model);
            return Json(new { Name = mAcc.Name, LRDetails = LRDetails, FMDetails = FMDetails, PartialDivName = PartialDivName, ReferAccReq = ReferAccReq, CostCenterTally = CostCenterTally }, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region Third Tab (Other Deduction)

        public ActionResult GetBreakDeduction(TripSheetVM Model)
        {
            if (Model.Mode == "Edit")
            {
                var result = (List<OtherExpenses>)Session["OtherDeductionList"];
                var result1 = result.Where(x => x.tempId == Model.otherExpenses.tempId);
                foreach (var item in result1)
                {
                    var Partial = "";
                    if (item.LRDetailList != null && item.LRDetailList.Count() > 0)
                    {
                        Partial = "LR";
                    }
                    if (item.FMDetailList != null && item.FMDetailList.Count() > 0)
                    {
                        Partial = "FM";
                    }
                    if (String.IsNullOrEmpty(Partial))
                    {
                        Partial = ctxTFAT.Master.Where(x => x.Code == item.ExpensesAc).Select(x => x.RelatedTo).FirstOrDefault();
                    }
                    Model.otherExpenses.ExpensesAc = item.ExpensesAc;
                    Model.otherExpenses.ExpensesAcName = item.ExpensesAcName;
                    Model.otherExpenses.DocNo = item.DocNo;
                    Model.otherExpenses.Amount = item.Amount;
                    Model.otherExpenses.Narr = item.Narr;
                    Model.otherExpenses.tempId = item.tempId;
                    Model.otherExpenses.RelatedTo = item.RelatedTo;
                    Model.PartialDivName = Partial;
                    Model.CostCenterTally = item.CostCenterTally;
                    Model.LRDetailList = item.LRDetailList;
                    Model.FMDetailList = item.FMDetailList;
                    Model.DectionCostCenter = true;
                }
                var jsonResult = Json(new { Html = this.RenderPartialView("AddNewDeduction", Model) }, JsonRequestBehavior.AllowGet);
                jsonResult.MaxJsonLength = int.MaxValue;
                return jsonResult;
            }
            else
            {
                Model.otherExpenses = new OtherExpenses();
                //var GetExpensesAccount = ctxTFAT.Master.Where(x => x.Code == Model.OtherDeductnAc).FirstOrDefault();
                //Model.otherExpenses.ExpensesAc = GetExpensesAccount.Code;
                //Model.otherExpenses.ExpensesAcName = GetExpensesAccount.Name;
                Model.otherExpenses.Amount = 0;
                Model.DectionCostCenter = true;
                var jsonResult = Json(new { Html = this.RenderPartialView("AddNewDeduction", Model) }, JsonRequestBehavior.AllowGet);
                jsonResult.MaxJsonLength = int.MaxValue;
                return jsonResult;
            }
        }

        [HttpPost]
        public ActionResult AddEditSelectedDeduction(TripSheetVM Model)
        {
            if (Model.Mode == "Add")
            {
                List<OtherExpenses> objledgerdetail = new List<OtherExpenses>();
                if (Session["OtherDeductionList"] != null)
                {
                    objledgerdetail = (List<OtherExpenses>)Session["OtherDeductionList"];
                }

                Model.otherExpenses.tempId = objledgerdetail.Count() + 1;
                objledgerdetail.Add(Model.otherExpenses);

                Session.Add("OtherDeductionList", objledgerdetail);
                Model.deductionlist = objledgerdetail;
                var html = ViewHelper.RenderPartialView(this, "OtherDeductionList", Model);
                return Json(new { Html = html }, JsonRequestBehavior.AllowGet);
            }
            else

            {
                var result = (List<OtherExpenses>)Session["OtherDeductionList"];

                var OtherExpenses = result.Where(x => x.tempId == Model.otherExpenses.tempId).FirstOrDefault();
                //OtherExpenses.DocNo = Model.otherExpenses.DocNo == null ? "" : Model.otherExpenses.DocNo;
                //OtherExpenses.Amount = Model.otherExpenses.Amount;
                //OtherExpenses.LRDetailList = Model.otherExpenses.LRDetailList;
                //OtherExpenses.FMDetailList = Model.otherExpenses.FMDetailList;


                OtherExpenses.ExpensesAc = Model.otherExpenses.ExpensesAc;
                OtherExpenses.ExpensesAcName = Model.otherExpenses.ExpensesAcName;
                OtherExpenses.DocNo = Model.otherExpenses.DocNo == null ? "" : Model.otherExpenses.DocNo;
                OtherExpenses.Amount = Model.otherExpenses.Amount;
                OtherExpenses.Narr = Model.otherExpenses.Narr;
                OtherExpenses.RelatedTo = Model.otherExpenses.RelatedTo;
                OtherExpenses.CostCenterTally = Model.otherExpenses.CostCenterTally;
                OtherExpenses.LRDetailList = Model.otherExpenses.LRDetailList;
                OtherExpenses.FMDetailList = Model.otherExpenses.FMDetailList;




                Model.deductionlist = result;

                var html = ViewHelper.RenderPartialView(this, "OtherDeductionList", Model);
                return Json(new
                {
                    Html = html
                }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult DeleteDeduction(TripSheetVM Model)
        {
            var result2 = (List<OtherExpenses>)Session["OtherDeductionList"];
            var result = result2.Where(x => x.tempId != Model.otherExpenses.tempId).ToList();
            int i = 1;
            foreach (var item in result)
            {
                item.tempId = i++;
            }
            Session.Add("OtherDeductionList", result);
            Model.deductionlist = result;
            var html = ViewHelper.RenderPartialView(this, "OtherDeductionList", Model);
            return Json(new { Html = html }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult RelatedToDecide(TripSheetVM Model)
        {
            string PartialDivName = "", html = "";
            var mAcc = ctxTFAT.Master.Where(x => x.Code == Model.Code).FirstOrDefault();

            var mIsVehicle = ctxTFAT.Master.Where(x => x.Code == Model.Code).Select(x => x.OthPostType).FirstOrDefault();

            if (mAcc.RelatedTo == "LR")
            {
                PartialDivName = "LR";
            }
            else if (mAcc.RelatedTo == "FM")
            {
                PartialDivName = "FM";
            }
            else
            {
                PartialDivName = "N";
            }

            var ReferAccReq = mAcc.ReferAccReq;
            var CostCenterTally = mAcc.CostCenterAmtTally;
            var LRDetails = this.RenderPartialView("LRDetailsD", Model);
            var FMDetails = this.RenderPartialView("FMDetailsD", Model);
            return Json(new { Name = mAcc.Name, LRDetails = LRDetails, FMDetails = FMDetails, PartialDivName = PartialDivName, ReferAccReq = ReferAccReq, CostCenterTally = CostCenterTally }, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region Fourth Tab(Summary)

        public ActionResult Summary(TripSheetVM Model)
        {

            //if (ctxTFAT.TripSheetSetup.Select(x => x.ShowSummary).FirstOrDefault())
            //{
            //    string mStr = @"select dbo.GetDriverTripBalance('" + Model.DriverCode + "','" + ConvertDDMMYYDate(Convert.ToDateTime(Model.FromDate)).ToString("yyyy/MM/dd HH:mm:ss") + "','D')";
            //    DataTable smDt = GetDataTable(mStr);
            //    if (smDt.Rows.Count > 0)
            //    {
            //        Model.DriverOpening = Convert.ToDecimal(smDt.Rows[0][0].ToString());
            //    }
            //    Model.ShowSummary = true;
            //}


            var html = ViewHelper.RenderPartialView(this, "Summary", Model);
            return Json(new { Html = html }, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region Posting Of Tripsheet

        public string GetAccName(string Code)
        {
            var mName = ctxTFAT.Master.Where(X => X.Code == Code).Select(X => X.Name).FirstOrDefault();
            return mName;
        }

        public string GetNewCode()
        {
            var mPrevSrl = GetLastSerial("TripSheetMaster", mbranchcode, "Trip0", mperiod, "JV", DateTime.Now.Date);

            //var mName = ctxTFAT.TripSheetMaster.Where(x => x.Prefix == mperiod).OrderByDescending(X => X.RECORDKEY).Select(X => X.DocNo).FirstOrDefault();
            //if (String.IsNullOrEmpty(mName))
            //{
            //    var DocType = ctxTFAT.DocTypes.Where(x => x.Code == "Trip0").Select(x => new { x.DocWidth, x.LimitFrom }).FirstOrDefault();
            //    mName = DocType.LimitFrom;
            //}
            //else
            //{
            //    mName = (Convert.ToInt32(mName) + 1).ToString("D6");
            //}
            return mPrevSrl;
        }

        public ActionResult GetPostingNew(TripSheetVM Model)
        {
            if (Model.Mode != "Delete")
            {
                //string mStr = CheckValidations(Model);
                //if (mStr != "")
                //{
                //    return Json(new
                //    {
                //        Message = mStr,
                //        Status = "ValidError"
                //    }, JsonRequestBehavior.AllowGet);
                //}
            }

            //string mValidation = GetValidationPosting(Model);
            //if (mValidation != "")
            //{
            //    return Json(new { Status = "ValidError", Message = mValidation }, JsonRequestBehavior.AllowGet);
            //}

            var TripFrom = ConvertDDMMYYTOYYMMDD(Model.FromDate);
            var TripTo = ConvertDDMMYYTOYYMMDD(Model.TODate);

            if (TripFrom != TripTo)
            {
                if (TripFrom > TripTo)
                {
                    return Json(new
                    {
                        Status = "ValidError",
                        Message = "Trip To Date Should Be Greater Than Trip From Date..."
                    }, JsonRequestBehavior.AllowGet);
                }
            }


            var Date = ConvertDDMMYYTOYYMMDD(Model.TripSheetDate);

            if (!(ConvertDDMMYYTOYYMMDD(StartDate) <= Date && Date <= ConvertDDMMYYTOYYMMDD(EndDate)))
            {
                return Json(new
                {
                    Status = "ValidError",
                    Message = "Document Date Should Be In  Selected Financial Year..."
                }, JsonRequestBehavior.AllowGet);
            }

            if (Model.VehicleFlag)
            {
                var Vehicle = ctxTFAT.VehicleMaster.Where(x => x.Code == Model.VehicleCode).FirstOrDefault();
                if (Vehicle != null)
                {
                    if (String.IsNullOrEmpty(Vehicle.PostAc))
                    {
                        return Json(new
                        {
                            Status = "ValidError",
                            Message = "Post Account Not Found In Vehicle Master..."
                        }, JsonRequestBehavior.AllowGet);
                    }
                }
            }
            else
            {
                var Driver = ctxTFAT.DriverMaster.Where(x => x.Code == Model.DriverCode).FirstOrDefault();
                if (Driver != null)
                {
                    if (String.IsNullOrEmpty(Driver.Posting))
                    {
                        return Json(new
                        {
                            Status = "ValidError",
                            Message = "Post Account Not Found In Driver Master..."
                        }, JsonRequestBehavior.AllowGet);
                    }
                }
            }

            List<PurchaseVM> LedPostList = new List<PurchaseVM>();

            string mDebitAcc = Model.TripDebitAc;

            string mCreditAcc = "";
            if (Model.VehicleFlag)
            {
                mCreditAcc = ctxTFAT.VehicleMaster.Where(x => x.Code == Model.VehicleCode).Select(x => x.PostAc).FirstOrDefault();
            }
            else
            {
                mCreditAcc = ctxTFAT.DriverMaster.Where(x => x.Code == Model.DriverCode).Select(x => x.Posting).FirstOrDefault();
            }


            if (Model.expenseslist == null)
            {
                Model.expenseslist = new List<OtherExpenses>();
            }
            if (Model.deductionlist == null)
            {
                Model.deductionlist = new List<OtherExpenses>();
            }

            if (Model.fMMasters == null)
            {
                Model.fMMasters = new List<FMMasterTrip>();
            }


            int xCnt = 1;

            if (ctxTFAT.TripSheetSetup.Select(x => x.SplitPostingReq).FirstOrDefault())
            {
                Model.NetAmt = Model.fMMasters.Sum(x => x.Total);
                Model.NetAmt += Model.TripChrgKMExp;
                //if (Model.CutAdv)
                //{
                //    Model.NetAmt -= Model.Advledgers == null ? 0 : Model.Advledgers.Sum(x => x.Debit) ?? 0;
                //}
                //if (Model.CutCC)
                {
                    Model.NetAmt -= Model.CCledgers == null ? 0 : Model.CCledgers.Where(x => x.Reminder == true).Sum(x => x.Debit) ?? 0;
                }
                for (int i = 1; i <= 2; i++)
                {
                    if (Model.NetAmt > 0)
                    {
                        if (i == 1)
                        {
                            LedPostList.Add(new PurchaseVM()
                            {
                                Code = mDebitAcc,
                                AccountName = GetAccName(mDebitAcc),
                                Debit = Model.NetAmt,
                                Credit = 0,
                                Branch = mbranchcode,
                                tempId = xCnt++,
                                DelyCode = mCreditAcc
                            });
                        }
                        if (i == 2)
                        {
                            LedPostList.Add(new PurchaseVM()
                            {
                                Code = mCreditAcc,
                                AccountName = GetAccName(mCreditAcc),
                                Debit = 0,
                                Credit = Model.NetAmt,
                                Branch = mbranchcode,
                                tempId = xCnt++,
                                DelyCode = mDebitAcc
                            });
                        }
                    }
                    else
                    {
                        if (i == 1)
                        {
                            LedPostList.Add(new PurchaseVM()
                            {
                                Code = mDebitAcc,
                                AccountName = GetAccName(mDebitAcc),
                                Debit = 0,
                                Credit = Model.NetAmt,
                                Branch = mbranchcode,
                                tempId = xCnt++,
                                DelyCode = mCreditAcc
                            });
                        }
                        if (i == 2)
                        {
                            LedPostList.Add(new PurchaseVM()
                            {
                                Code = mCreditAcc,
                                AccountName = GetAccName(mCreditAcc),
                                Debit = Model.NetAmt,
                                Credit = 0,
                                Branch = mbranchcode,
                                tempId = xCnt++,
                                DelyCode = mDebitAcc
                            });
                        }
                    }
                }
                if (Model.expenseslist.ToList().Count() > 0)
                {
                    var DistinctExp = Model.expenseslist.Select(x => x.ExpensesAc).ToList().Distinct();
                    foreach (var item in DistinctExp)
                    {
                        LedPostList.Add(new PurchaseVM()
                        {
                            Code = item,
                            AccountName = GetAccName(item),
                            Debit = Model.expenseslist.Where(x => x.ExpensesAc == item).Sum(x => x.Amount),
                            Credit = 0,
                            Branch = mbranchcode,
                            tempId = xCnt++,
                            DelyCode = mCreditAcc
                        });
                    }
                    LedPostList.Add(new PurchaseVM()
                    {
                        Code = mCreditAcc,
                        AccountName = GetAccName(mCreditAcc),
                        Debit = 0,
                        Credit = Model.expenseslist.Where(x => DistinctExp.Contains(x.ExpensesAc)).Sum(x => x.Amount),
                        Branch = mbranchcode,
                        tempId = xCnt++,
                        DelyCode = String.Join(",", DistinctExp)
                    });
                }
                if (Model.deductionlist.ToList().Count() > 0)
                {
                    var DistinctExpDed = Model.deductionlist.Select(x => x.ExpensesAc).ToList().Distinct();
                    LedPostList.Add(new PurchaseVM()
                    {
                        Code = mCreditAcc,
                        AccountName = GetAccName(mCreditAcc),
                        Debit = Model.deductionlist.Where(x => DistinctExpDed.Contains(x.ExpensesAc)).Sum(x => x.Amount),
                        Credit = 0,
                        Branch = mbranchcode,
                        tempId = xCnt++,
                        DelyCode = String.Join(",", DistinctExpDed)
                    });
                    foreach (var item in DistinctExpDed)
                    {
                        LedPostList.Add(new PurchaseVM()
                        {
                            Code = item,
                            AccountName = GetAccName(item),
                            Debit = 0,
                            Credit = Model.deductionlist.Where(x => x.ExpensesAc == item).Sum(x => x.Amount),
                            Branch = mbranchcode,
                            tempId = xCnt++,
                            DelyCode = mCreditAcc
                        });
                    }
                }
            }
            else
            {
                Model.NetAmt = (Model.fMMasters.Sum(x => x.Total) + Model.expenseslist.Sum(x => x.Amount)) - Model.deductionlist.Sum(x => x.Amount);
                //if (Model.CutAdv)
                //{
                //    Model.NetAmt -= Model.Advledgers == null ? 0 : Model.Advledgers.Sum(x => x.Debit) ?? 0;
                //}
                //if (Model.CutCC)
                {
                    Model.NetAmt -= Model.CCledgers == null ? 0 : Model.CCledgers.Where(x => x.Reminder == true).Sum(x => x.Debit) ?? 0;
                }
                if (Model.NetAmt > 0)
                {
                    for (int i = 1; i <= 2; i++)
                    {
                        if (i == 1)
                        {
                            LedPostList.Add(new PurchaseVM()
                            {
                                Code = mDebitAcc,
                                AccountName = GetAccName(mDebitAcc),
                                Debit = Model.NetAmt,
                                Credit = 0,
                                Branch = mbranchcode,
                                tempId = xCnt++,
                                DelyCode = mCreditAcc
                            });
                        }

                        if (i == 2)
                        {
                            LedPostList.Add(new PurchaseVM()
                            {
                                Code = mCreditAcc,
                                AccountName = GetAccName(mCreditAcc),
                                Debit = 0,
                                Credit = Model.NetAmt,
                                Branch = mbranchcode,
                                tempId = xCnt++,
                                DelyCode = mDebitAcc
                                //RefDoc = "B"
                            });
                        }
                    }
                }
                else
                {
                    for (int i = 1; i <= 2; i++)
                    {
                        if (i == 1)
                        {
                            LedPostList.Add(new PurchaseVM()
                            {
                                Code = mDebitAcc,
                                AccountName = GetAccName(mDebitAcc),
                                Debit = 0,
                                Credit = Model.NetAmt,
                                Branch = mbranchcode,
                                tempId = xCnt++,
                                DelyCode = mCreditAcc
                            });
                        }

                        if (i == 2)
                        {
                            LedPostList.Add(new PurchaseVM()
                            {
                                Code = mCreditAcc,
                                AccountName = GetAccName(mCreditAcc),
                                Debit = Model.NetAmt,
                                Credit = 0,
                                Branch = mbranchcode,
                                tempId = xCnt++,
                                DelyCode = mDebitAcc
                                //RefDoc = "B"
                            });
                        }
                    }
                }

            }

            if (Model.CutTDS == true && Model.TDSAmt > 0)
            {
                LedPostList.Add(new PurchaseVM()
                {
                    Code = mCreditAcc,
                    AccountName = GetAccName(mCreditAcc),
                    Debit = Model.TDSAmt,
                    Credit = 0,
                    Branch = mbranchcode,
                    tempId = xCnt++,
                    DelyCode = "000009994"
                });
                LedPostList.Add(new PurchaseVM()
                {
                    Code = "000009994",
                    AccountName = GetAccName("000009994"),
                    Debit = 0,
                    Credit = Model.TDSAmt,
                    Branch = mbranchcode,
                    tempId = xCnt++,
                    DelyCode = mCreditAcc
                });
            }


            Model.TotDebit = LedPostList.Sum(x => x.Debit);
            Model.TotCredit = LedPostList.Sum(x => x.Credit);
            if (Model.TotDebit < 0)
            {
                Model.TotDebit = (-1) * Model.TotDebit;
            }
            if (Model.TotCredit < 0)
            {
                Model.TotCredit = (-1) * Model.TotCredit;
            }
            Model.LedgerPostList = LedPostList;

            // display view form
            var html = ViewHelper.RenderPartialView(this, "LedgerPosting", new TripSheetVM() { LedgerPostList = Model.LedgerPostList, TotDebit = Math.Round(Model.TotDebit, 2), TotCredit = Math.Round(Model.TotCredit, 2), Mode = Model.Mode, Controller2 = Model.Controller2, Module = Model.Module, ViewDataId = Model.ViewDataId, TableName = Model.TableName, Header = Model.Header, OptionType = Model.OptionType, OptionCode = Model.OptionCode, MainType = Model.MainType });
            return Json(new
            {
                LedgerPostList = Model.LedgerPostList,
                TotDebit = Math.Round(Model.TotDebit, 2),
                TotCredit = Math.Round(Model.TotCredit, 2),
                Mode = Model.Mode,
                Html = html,
                Status = "Success"
            }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult SaveData(TripSheetVM mModel)
        {
            string NewSrl = "";
            using (var transaction = ctxTFAT.Database.BeginTransaction())
            {
                try
                {
                    if (mModel.deductionlist == null)
                    {
                        mModel.deductionlist = new List<OtherExpenses>();
                    }
                    if (mModel.expenseslist == null)
                    {
                        mModel.expenseslist = new List<OtherExpenses>();
                    }




                    // iX9: Remove Existing Data for Delete Mode
                    if (mModel.Mode == "Delete")
                    {
                        var Docu = ctxTFAT.TripSheetMaster.Where(x => x.DocNo == mModel.Document && x.Prefix == mperiod).FirstOrDefault();
                        mModel.TableKey = Docu.TableKey;
                        mModel.ParentKey = Docu.ParentKey;
                        string Msg = DeleteStateMaster(mModel);
                        transaction.Commit();
                        transaction.Dispose();
                        if (Msg == "Success")
                        {
                            if (mModel.VehicleFlag == true)
                            {
                                UpdateAuditTrail(mbranchcode, mModel.Mode, mModel.Header, mModel.ParentKey, Docu.DocDate, mModel.NetAmt, Docu.Driver, "Delete Trip Sheet", "VM");
                            }
                            else
                            {
                                UpdateAuditTrail(mbranchcode, mModel.Mode, mModel.Header, mModel.ParentKey, Docu.DocDate, mModel.NetAmt, Docu.Driver, "Delete Trip Sheet", "DM");
                            }
                            return Json(new { Status = "Success", Message = "Data is Deleted." }, JsonRequestBehavior.AllowGet);
                        }
                        else
                        {
                            return Json(new { Status = "Error", Message = Msg }, JsonRequestBehavior.AllowGet);
                        }
                    }
                    if (mModel.Mode == "Edit")
                    {
                        var Demo = ctxTFAT.TripSheetMaster.Where(x => x.DocNo == mModel.Document && x.Prefix == mperiod).FirstOrDefault();
                        if (mbranchcode != Demo.Branch)
                        {
                            transaction.Rollback();
                            return Json(new { Message = "Login Branch Does Not Match With Documnet Branch.", Status = "Error", id = "LRMaster" }, JsonRequestBehavior.AllowGet);
                        }
                    }

                    bool mAdd = true;
                    TripSheetMaster mobj = new TripSheetMaster();
                    mModel.Type = "Trip0";
                    mModel.MainType = "JV";
                    mModel.SubType = "GJ";

                    if (ctxTFAT.TripSheetMaster.Where(x => x.DocNo == mModel.Document && x.Prefix == mperiod).FirstOrDefault() != null)
                    {
                        mobj = ctxTFAT.TripSheetMaster.Where(x => x.DocNo == mModel.Document && x.Prefix == mperiod).FirstOrDefault();
                        mAdd = false;
                        mModel.TableKey = mobj.TableKey;
                        mModel.ParentKey = mobj.ParentKey;
                        DeUpdate(mModel);
                        NewSrl = mobj.DocNo + mobj.Prefix;
                    }
                    if (mAdd == true)
                    {
                        if (String.IsNullOrEmpty(mModel.TripSheetNo))
                        {
                            mobj.DocNo = GetNewCode().Trim();
                        }
                        else
                        {
                            var result1 = ctxTFAT.DocTypes.Where(x => x.Code == "Trip0").Select(x => x).FirstOrDefault();
                            mobj.DocNo = mModel.TripSheetNo.PadLeft(result1.DocWidth, '0').Trim();
                        }
                        mobj.EntryDate = DateTime.Now;
                        mobj.Prefix = mperiod;
                        mobj.ParentKey = mbranchcode + "Trip0" + mperiod.Substring(0, 2) + mobj.DocNo;
                        mobj.TableKey = mbranchcode + "Trip0" + mperiod.Substring(0, 2) + 1.ToString("D3") + mobj.DocNo;
                        mModel.ParentKey = mobj.ParentKey;

                        if (ctxTFAT.TripSheetMaster.Where(x => x.DocNo == mobj.DocNo && x.Prefix == mperiod).FirstOrDefault() != null)
                        {
                            return Json(new { Status = "Error", Message = "This Doc.Serial Already Exist Please Change The Serial NO...!" }, JsonRequestBehavior.AllowGet);
                        }

                    }

                    if (mModel.CCledgers == null)
                    {
                        mModel.CCledgers = new List<Ledger>();
                    }
                    if (mModel.Advledgers == null)
                    {
                        mModel.Advledgers = new List<Ledger>();
                    }
                    if (mModel.Balledgers == null)
                    {
                        mModel.Balledgers = new List<Ledger>();
                    }
                    if (mModel.fMMasters == null)
                    {
                        mModel.fMMasters = new List<FMMasterTrip>();
                    }
                    if (mModel.fMMasters.Count() > 0)
                    {
                        if (!mModel.FMOrNOt)
                        {
                            var DocString = mModel.FMOrNOt == false ? "LRNo : " : "FMNo : ";
                            mModel.Narr += DocString;
                            foreach (var item in mModel.fMMasters.Select(x => x.FMNo).Distinct().ToList())
                            {
                                mModel.Narr += item + ",";
                            }
                            mModel.Narr = mModel.Narr.Substring(0, mModel.Narr.Length - 1);
                            mModel.Narr += "      VehicleNo : ";
                            foreach (var item in mModel.fMMasters.Select(x => x.VehicleNo).Distinct().ToList())
                            {
                                mModel.Narr += item + ",";
                            }
                            mModel.Narr = mModel.Narr.Substring(0, mModel.Narr.Length - 1);
                        }
                    }





                    #region Authorisation
                    var Amou = mModel.TripChrgKMExp + Convert.ToDecimal((mModel.fMMasters.Sum(x => x.Total) + mModel.expenseslist.Sum(x => (decimal?)x.Amount) ?? 0) - (mModel.deductionlist.Sum(x => (decimal?)x.Amount) ?? 0)) - (mModel.CCledgers.Where(x => x.Reminder == true).Sum(x => (decimal?)x.Debit) ?? 0);
                    string Post = "";
                    if (mModel.VehicleFlag)
                    {
                        Post = ctxTFAT.VehicleMaster.Where(x => x.Code == mModel.VehicleCode).Select(x => x.PostAc).FirstOrDefault();
                    }
                    else
                    {
                        Post = ctxTFAT.DriverMaster.Where(x => x.Code == mModel.DriverCode).Select(x => x.Posting).FirstOrDefault();
                    }
                    TfatUserAuditHeader authorisation = ctxTFAT.TfatUserAuditHeader.Where(x => x.Type == "Trip0").FirstOrDefault();
                    if (authorisation != null)
                    {
                        mauthorise = SetAuthorisationLogistics(authorisation, mModel.ParentKey, mobj.DocNo.ToString(), 0, mModel.TripSheetDate, Amou, Post, mbranchcode);
                    }
                    #endregion

                    mobj.DocDate = ConvertDDMMYYTOYYMMDD(mModel.TripSheetDate);
                    mobj.Branch = mbranchcode;
                    mobj.Driver = mModel.VehicleFlag == false ? mModel.DriverCode : mModel.VehicleCode;
                    mobj.DebitAc = mModel.TripDebitAc;
                    mobj.TDSAmt = mModel.TDSAmt;
                    mobj.NetAmt = mModel.TripChrgKMExp + Convert.ToDecimal((mModel.fMMasters.Sum(x => x.Total) + mModel.expenseslist.Sum(x => (decimal?)x.Amount) ?? 0) - (mModel.deductionlist.Sum(x => (decimal?)x.Amount) ?? 0)) - (mModel.CCledgers.Where(x => x.Reminder == true).Sum(x => (decimal?)x.Debit) ?? 0);
                    mobj.Narr = mModel.Narr;
                    mobj.FromKM = mModel.FromKM;
                    mobj.ToKM = mModel.ToKM;
                    mobj.PerKMChrg = mModel.PerKMChrg;
                    mobj.FromDT = ConvertDDMMYYTOYYMMDD(mModel.FromDate);
                    mobj.TODT = ConvertDDMMYYTOYYMMDD(mModel.TODate);
                    mobj.VehicleFlag = mModel.VehicleFlag;
                    var AdjustLedgerRef = "";
                    if (mModel.Advledgers != null)
                    {
                        if (mModel.Advledgers.Count() > 0)
                        {
                            foreach (var item in mModel.Advledgers)
                            {
                                AdjustLedgerRef += item.Branch + item.TableKey + "^";
                            }
                        }
                    }
                    if (!String.IsNullOrEmpty(AdjustLedgerRef))
                    {
                        AdjustLedgerRef = AdjustLedgerRef.Substring(0, AdjustLedgerRef.Length - 1);
                    }
                    mobj.AdjustLedgerRef = AdjustLedgerRef;

                    var BalAdjustLedgerRef = "";
                    if (mModel.Balledgers != null)
                    {
                        if (mModel.Balledgers.Count() > 0)
                        {
                            foreach (var item in mModel.Balledgers)
                            {
                                BalAdjustLedgerRef += item.Branch + item.TableKey + "^";
                            }
                        }
                    }
                    if (!String.IsNullOrEmpty(BalAdjustLedgerRef))
                    {
                        BalAdjustLedgerRef = BalAdjustLedgerRef.Substring(0, BalAdjustLedgerRef.Length - 1);
                    }
                    mobj.AdjustBalLedgerRef = BalAdjustLedgerRef;


                    var CCAdjustLedgerRef = "";
                    var CCAdjustLedgerRefCut = "";
                    if (mModel.CCledgers != null)
                    {
                        if (mModel.CCledgers.Count() > 0)
                        {
                            foreach (var item in mModel.CCledgers)
                            {
                                if (item.TDSFlag == true)
                                {
                                    CCAdjustLedgerRef += item.Branch + item.TableKey + "^";
                                }
                                if (item.Reminder == true)
                                {
                                    CCAdjustLedgerRefCut += item.Branch + item.TableKey + "^";
                                }

                            }
                        }
                    }
                    if (!String.IsNullOrEmpty(CCAdjustLedgerRef))
                    {
                        CCAdjustLedgerRef = CCAdjustLedgerRef.Substring(0, CCAdjustLedgerRef.Length - 1);
                    }
                    mobj.CCjustLedgerRef = CCAdjustLedgerRef;

                    if (!String.IsNullOrEmpty(CCAdjustLedgerRefCut))
                    {
                        CCAdjustLedgerRefCut = CCAdjustLedgerRefCut.Substring(0, CCAdjustLedgerRefCut.Length - 1);
                    }
                    mobj.CCLedgerRefCutFromTrip = CCAdjustLedgerRefCut;


                    mobj.AdvFromDT = ConvertDDMMYYTOYYMMDD(mModel.AdvFromDate);
                    mobj.AdvTODT = ConvertDDMMYYTOYYMMDD(mModel.AdvTODate);

                    mobj.BalFromDT = ConvertDDMMYYTOYYMMDD(mModel.BalFromDate);
                    mobj.BalTODT = ConvertDDMMYYTOYYMMDD(mModel.BalTODate);

                    mobj.CCFromDT = ConvertDDMMYYTOYYMMDD(mModel.CCFromDate);
                    mobj.CCTODT = ConvertDDMMYYTOYYMMDD(mModel.CCTODate);

                    mobj.CutAdv = mModel.CutAdv;
                    mobj.CutCC = mModel.CutCC;

                    mobj.FMorNOT = mModel.FMOrNOt;

                    mobj.LASTUPDATEDATE = ConvertDDMMYYTOYYMMDD(DateTime.Now.ToString());
                    mobj.ENTEREDBY = muserid;
                    mobj.AUTHORISE = mauthorise;
                    mobj.AUTHIDS = muserid;

                    if (mAdd == true)
                    {
                        ctxTFAT.TripSheetMaster.Add(mobj);
                        NewSrl = mobj.DocNo + mobj.Prefix;
                    }
                    else
                    {
                        ctxTFAT.Entry(mobj).State = EntityState.Modified;
                    }

                    #region Fm List Of TripSheet

                    List<TripFmList> tripFmLists = new List<TripFmList>();
                    foreach (var item in mModel.fMMasters)
                    {
                        tripFmLists.Add(new TripFmList
                        {
                            DocNo = mobj.DocNo,
                            Prefix = mperiod,
                            RefTablekey = item.RefTablekey,
                            FMNo = item.FMNo,
                            FmDate = item.Date,
                            VehicleNo = String.IsNullOrEmpty(item.VehicleNo) == true ? "" : String.IsNullOrEmpty(ctxTFAT.VehicleMaster.Where(x => x.TruckNo == item.VehicleNo).Select(x => x.Code).FirstOrDefault()) == true ? ctxTFAT.HireVehicleMaster.Where(x => x.TruckNo == item.VehicleNo).Select(x => x.Code).FirstOrDefault() : ctxTFAT.VehicleMaster.Where(x => x.TruckNo == item.VehicleNo).Select(x => x.Code).FirstOrDefault(),
                            FromBranch = item.From,
                            ToBranch = item.To,
                            RouteVia = item.RouteVia,
                            TripChrg = item.Tripchages,
                            LocalChrg = item.LocalCharges,
                            ViaChrg = item.ViaCharges,
                            Total = item.Total,
                            ENTEREDBY = muserid,
                            AUTHORISE = mauthorise,
                            AUTHIDS = muserid,
                            LASTUPDATEDATE = ConvertDDMMYYTOYYMMDD(DateTime.Now.ToString())
                        });
                    }
                    ctxTFAT.TripFmList.AddRange(tripFmLists);

                    #endregion

                    #region Trip Expenses

                    List<TripExpensesMaster> tripExpensesMasters = new List<TripExpensesMaster>();
                    int xCnt = 1;

                    var GetLrID = ctxTFAT.RelLr.OrderByDescending(x => x.LrID).Select(x => x.LrID).FirstOrDefault();
                    int GetLrID1 = (Convert.ToInt32(GetLrID) + 1);

                    var GetFmID = ctxTFAT.RelFm.OrderByDescending(x => x.FMID).Select(x => x.FMID).FirstOrDefault();
                    int GetFmID1 = (Convert.ToInt32(GetFmID) + 1);

                    mModel.expenseslist = (List<OtherExpenses>)Session["OtherExpensesList"];
                    mModel.deductionlist = (List<OtherExpenses>)Session["OtherDeductionList"];

                    if (mModel.expenseslist == null)
                    {
                        mModel.expenseslist = new List<OtherExpenses>();
                    }
                    if (mModel.deductionlist == null)
                    {
                        mModel.deductionlist = new List<OtherExpenses>();
                    }
                    xCnt = 1;
                    foreach (var item in mModel.expenseslist)
                    {


                        GetLrID = GetLrID1.ToString();
                        if (GetLrID.Length > 6)
                        {
                            GetLrID.PadLeft(6, '0');
                        }

                        GetFmID = GetFmID1.ToString();
                        if (GetFmID.Length > 6)
                        {
                            GetFmID.PadLeft(6, '0');
                        }

                        if (item.LRDetailList == null)
                        {
                            item.LRDetailList = new List<TripSheetVM>();
                        }
                        if (item.FMDetailList == null)
                        {
                            item.FMDetailList = new List<TripSheetVM>();
                        }


                        tripExpensesMasters.Add(new TripExpensesMaster
                        {
                            DocNo = mobj.DocNo,
                            Prefix = mperiod,
                            Account = item.ExpensesAc,
                            DocRefCode = item.DocNo,
                            RefType = item.RelatedTo,
                            DebitAmt = item.Amount,
                            CreditAmt = 0,
                            ENTEREDBY = muserid,
                            AUTHORISE = mauthorise,
                            AUTHIDS = muserid,
                            Branch = mbranchcode,
                            ParentKey = mobj.ParentKey,
                            TableKey = mbranchcode + "Trip0" + mperiod.Substring(0, 2) + xCnt.ToString("D3") + mobj.DocNo.ToString(),
                            LASTUPDATEDATE = ConvertDDMMYYTOYYMMDD(DateTime.Now.ToString()),
                            Narr = item.Narr
                        });

                        RelateData reldt = new RelateData();
                        reldt.Amount = item.Amount;
                        reldt.AUTHIDS = muserid;
                        reldt.AUTHORISE = mauthorise;
                        reldt.Branch = mbranchcode;
                        reldt.DocDate = ConvertDDMMYYTOYYMMDD(mModel.TripSheetDate);
                        reldt.ENTEREDBY = muserid;
                        reldt.Deleted = false;
                        reldt.Type = "Trip0";
                        reldt.Srl = Convert.ToInt32(mobj.DocNo);
                        reldt.Sno = xCnt.ToString("D3");
                        reldt.SubType = "GJ";
                        reldt.LASTUPDATEDATE = DateTime.Now;
                        reldt.MainType = "JV";
                        reldt.Code = item.ExpensesAc;
                        reldt.Narr = "";
                        //reldt.LrId = item.RelatedTo == "LR" ? item.DocNo : item.RelatedTo == "FM" ? item.DocNo : null;
                        reldt.RelateTo = item.RelatedTo == "LR" ? (byte)(3) : item.RelatedTo == "FM" ? (byte)(5) : (byte)(0);
                        //reldt.Value8 = li.RelatedChoice;
                        //reldt.Combo1 = li.RelatedTo;
                        reldt.TableKey = mbranchcode + "Trip0" + mperiod.Substring(0, 2) + xCnt.ToString("D3") + mobj.DocNo.ToString();
                        reldt.ParentKey = mobj.TableKey;
                        reldt.AmtType = true;
                        reldt.Clear = false;
                        reldt.Status = false;
                        ctxTFAT.RelateData.Add(reldt);

                        int xrellrCnt = 1;
                        if (item.LRDetailList != null && item.LRDetailList.Count > 0)
                        {
                            foreach (var l in item.LRDetailList)
                            {
                                RelLr rllr = new RelLr();
                                rllr.AUTHIDS = muserid;
                                rllr.AUTHORISE = mauthorise;
                                rllr.Branch = mbranchcode;
                                rllr.Deleted = false;
                                rllr.ENTEREDBY = muserid;
                                rllr.LASTUPDATEDATE = DateTime.Now;
                                rllr.LrAmt = l.LRAmt;
                                rllr.LrID = GetLrID;
                                rllr.LrNo = l.LRNumber;
                                rllr.TableKey = mbranchcode + "Trip0" + mperiod.Substring(0, 2) + xCnt.ToString("D3") + mobj.DocNo.ToString();
                                rllr.SrNo = xrellrCnt;
                                rllr.ParentKey = mobj.TableKey;
                                rllr.LRRefTablekey = l.ConsignmentKey;
                                rllr.Prefix = mperiod;
                                ctxTFAT.RelLr.Add(rllr);
                                xrellrCnt = xrellrCnt + 1;
                            }
                            ++GetLrID1;
                        }
                        if (item.FMDetailList != null && item.FMDetailList.Count > 0)
                        {
                            xrellrCnt = 1;
                            foreach (var l in item.FMDetailList)
                            {
                                RelFm rllr = new RelFm();
                                rllr.AUTHIDS = muserid;
                                rllr.AUTHORISE = mauthorise;
                                rllr.Branch = mbranchcode;
                                rllr.ENTEREDBY = muserid;
                                rllr.LASTUPDATEDATE = DateTime.Now;
                                rllr.FmAmt = l.FMAmt;
                                rllr.FMNo = l.FMNumber;
                                rllr.FMID = GetFmID;
                                rllr.TableKey = mbranchcode + "Trip0" + mperiod.Substring(0, 2) + xCnt.ToString("D3") + mobj.DocNo.ToString();
                                rllr.SrNo = xrellrCnt;
                                rllr.ParentKey = mobj.TableKey;
                                rllr.FMRefTablekey = l.FreightMemoKey;
                                rllr.Prefix = mperiod;
                                ctxTFAT.RelFm.Add(rllr);
                                xrellrCnt = xrellrCnt + 1;
                            }
                            ++GetFmID1;
                        }
                        ++xCnt;
                    }
                    ctxTFAT.TripExpensesMaster.AddRange(tripExpensesMasters);

                    #endregion

                    #region Trip Expenses Deduction

                    List<TripExpensesMaster> tripDeductionMasters = new List<TripExpensesMaster>();
                    foreach (var item in mModel.deductionlist)
                    {
                        GetLrID = GetLrID1.ToString();
                        if (GetLrID.Length > 6)
                        {
                            GetLrID.PadLeft(6, '0');
                        }

                        GetFmID = GetFmID1.ToString();
                        if (GetFmID.Length > 6)
                        {
                            GetFmID.PadLeft(6, '0');
                        }
                        if (item.LRDetailList == null)
                        {
                            item.LRDetailList = new List<TripSheetVM>();
                        }
                        if (item.FMDetailList == null)
                        {
                            item.FMDetailList = new List<TripSheetVM>();
                        }


                        tripDeductionMasters.Add(new TripExpensesMaster
                        {
                            DocNo = mobj.DocNo,
                            Prefix = mperiod,
                            Account = item.ExpensesAc,
                            DocRefCode = item.DocNo,
                            RefType = item.RelatedTo,
                            DebitAmt = 0,
                            CreditAmt = item.Amount,
                            ENTEREDBY = muserid,
                            AUTHORISE = mauthorise,
                            AUTHIDS = muserid,
                            Branch = mbranchcode,
                            ParentKey = mobj.ParentKey,
                            TableKey = mbranchcode + "Trip0" + mperiod.Substring(0, 2) + xCnt.ToString("D3") + mobj.DocNo.ToString(),
                            LASTUPDATEDATE = ConvertDDMMYYTOYYMMDD(DateTime.Now.ToString()),
                            Narr = item.Narr
                        });
                        RelateData reldt = new RelateData();
                        reldt.Amount = item.Amount;
                        reldt.AUTHIDS = muserid;
                        reldt.AUTHORISE = mauthorise;
                        reldt.Branch = mbranchcode;
                        reldt.DocDate = ConvertDDMMYYTOYYMMDD(mModel.TripSheetDate);
                        reldt.ENTEREDBY = muserid;
                        reldt.Deleted = false;
                        reldt.Type = "Trip0";
                        reldt.Srl = Convert.ToInt32(mobj.DocNo);
                        reldt.Sno = xCnt.ToString("D3");
                        reldt.SubType = "GJ";
                        reldt.LASTUPDATEDATE = DateTime.Now;
                        reldt.MainType = "JV";
                        reldt.Code = item.ExpensesAc;
                        reldt.Narr = "";
                        //reldt.LrId = item.RelatedTo == "LR" ? item.DocNo: item.RelatedTo == "FM" ? item.DocNo : null;
                        reldt.RelateTo = item.RelatedTo == "LR" ? (byte)(3) : item.RelatedTo == "FM" ? (byte)(5) : (byte)(0);
                        //reldt.Value8 = li.RelatedChoice;
                        //reldt.Combo1 = li.RelatedTo;
                        reldt.TableKey = mbranchcode + "Trip0" + mperiod.Substring(0, 2) + xCnt.ToString("D3") + mobj.DocNo.ToString();
                        reldt.ParentKey = mobj.TableKey;
                        reldt.AmtType = true;
                        reldt.Clear = false;
                        reldt.Status = false;
                        ctxTFAT.RelateData.Add(reldt);

                        int xrellrCnt = 1;
                        if (item.LRDetailList != null && item.LRDetailList.Count > 0)
                        {
                            foreach (var l in item.LRDetailList)
                            {
                                RelLr rllr = new RelLr();
                                rllr.AUTHIDS = muserid;
                                rllr.AUTHORISE = mauthorise;
                                rllr.Branch = mbranchcode;
                                rllr.Deleted = false;
                                rllr.ENTEREDBY = muserid;
                                rllr.LASTUPDATEDATE = DateTime.Now;
                                rllr.LrAmt = l.LRAmt;
                                rllr.LrID = GetLrID;
                                rllr.LrNo = l.LRNumber;
                                rllr.TableKey = mbranchcode + "Trip0" + mperiod.Substring(0, 2) + xCnt.ToString("D3") + mobj.DocNo.ToString();
                                rllr.SrNo = xrellrCnt;
                                rllr.ParentKey = mobj.TableKey;
                                rllr.LRRefTablekey = l.ConsignmentKey;
                                rllr.Prefix = mperiod;
                                ctxTFAT.RelLr.Add(rllr);
                                xrellrCnt = xrellrCnt + 1;
                            }
                            ++GetLrID1;
                        }
                        if (item.FMDetailList != null && item.FMDetailList.Count > 0)
                        {
                            xrellrCnt = 1;
                            foreach (var l in item.FMDetailList)
                            {
                                RelFm rllr = new RelFm();
                                rllr.AUTHIDS = muserid;
                                rllr.AUTHORISE = mauthorise;
                                rllr.Branch = mbranchcode;
                                rllr.ENTEREDBY = muserid;
                                rllr.LASTUPDATEDATE = DateTime.Now;
                                rllr.FmAmt = l.FMAmt;
                                rllr.FMNo = l.FMNumber;
                                rllr.FMID = GetFmID;
                                rllr.TableKey = mbranchcode + "Trip0" + mperiod.Substring(0, 2) + xCnt.ToString("D3") + mobj.DocNo.ToString();
                                rllr.SrNo = xrellrCnt;
                                rllr.ParentKey = mobj.TableKey;
                                rllr.FMRefTablekey = l.FreightMemoKey;
                                rllr.Prefix = mperiod;
                                ctxTFAT.RelFm.Add(rllr);
                                xrellrCnt = xrellrCnt + 1;
                            }
                            ++GetFmID1;
                        }
                        ++xCnt;
                    }
                    ctxTFAT.TripExpensesMaster.AddRange(tripDeductionMasters);

                    #endregion

                    #region Posting

                    List<PurchaseVM> LedgerPosting = mModel.LedgerPostList;
                    List<Ledger> ledgers = new List<Ledger>();
                    int lCnt = 1;
                    var ledpost = LedgerPosting;
                    if (ledpost != null)
                    {
                        //string mauthorise = "A00";
                        for (int u = 0; u < ledpost.Count; u++)
                        {
                            Ledger mobjL = new Ledger();
                            mobjL.AltCode = ledpost[u].DelyCode;
                            mobjL.Audited = true;
                            mobjL.AUTHIDS = muserid;
                            mobjL.AUTHORISE = mauthorise;
                            //mobjL.AUTHORISE = "A00";
                            mobjL.BillDate = ConvertDDMMYYTOYYMMDD(mModel.TripSheetDate.ToString());
                            mobjL.BillNumber = "";
                            mobjL.Branch = mbranchcode;
                            mobjL.Cheque = "";
                            mobjL.ChequeReturn = false;
                            mobjL.ChqCategory = 1;
                            mobjL.ClearDate = DateTime.Now;
                            mobjL.Code = ledpost[u].Code;
                            mobjL.Credit = Convert.ToDecimal(ledpost[u].Credit);
                            mobjL.CrPeriod = 0;
                            mobjL.CurrAmount = Convert.ToDecimal(ledpost[u].Credit + ledpost[u].Debit);
                            mobjL.CurrName = 1;
                            mobjL.CurrRate = 1;
                            mobjL.Debit = Convert.ToDecimal(ledpost[u].Debit);
                            mobjL.Discounted = true;
                            mobjL.DocDate = ConvertDDMMYYTOYYMMDD(mModel.TripSheetDate.ToString());
                            mobjL.DueDate = DateTime.Now;
                            mobjL.LocationCode = 100001;
                            mobjL.MainType = "JV";
                            mobjL.Narr = mModel.Narr;
                            mobjL.Party = ledpost[u].Code;
                            mobjL.Prefix = mperiod;
                            mobjL.RecoFlag = "";
                            mobjL.RefDoc = ledpost[u].RefDoc;
                            mobjL.Reminder = true;
                            mobjL.Sno = lCnt;
                            mobjL.Srl = mobj.DocNo;
                            mobjL.SubType = "GJ";
                            mobjL.TaskID = 0;
                            mobjL.TDSChallanNumber = "";
                            mobjL.TDSCode = Convert.ToInt32(mModel.TDSCode);
                            mobjL.TDSFlag = mModel.CutTDS;
                            mobjL.Type = "Trip0";
                            mobjL.ENTEREDBY = muserid;
                            mobjL.LASTUPDATEDATE = DateTime.Now;
                            mobjL.ChequeDate = DateTime.Now;
                            mobjL.CompCode = mcompcode;
                            mobjL.ParentKey = mobj.ParentKey;
                            mobjL.TableKey = mbranchcode + "Trip0" + mperiod.Substring(0, 2) + lCnt.ToString("D3") + mobj.DocNo;
                            mobjL.PCCode = 100002;
                            ctxTFAT.Ledger.Add(mobjL);
                            ledgers.Add(mobjL);

                            ++lCnt;
                        }
                    }

                    #endregion

                    #region TDS Outstanding

                    if (mModel.CutTDS == true && mModel.TDSAmt > 0)
                    {
                        string mCreditAcc = "";
                        if (mModel.VehicleFlag)
                        {
                            mCreditAcc = ctxTFAT.VehicleMaster.Where(x => x.Code == mModel.VehicleCode).Select(x => x.PostAc).FirstOrDefault();
                        }
                        else
                        {
                            mCreditAcc = ctxTFAT.DriverMaster.Where(x => x.Code == mModel.DriverCode).Select(x => x.Posting).FirstOrDefault();
                        }
                        var DriverLedger = ledgers.Where(x => x.AltCode == mModel.TripDebitAc && x.Code == mCreditAcc).Select(x => new { x.TableKey, x.Sno }).FirstOrDefault();
                        var TdsLedgerLedger = ledgers.Where(x => x.AltCode == "000009994" && x.Code == mCreditAcc).Select(x => new { x.TableKey, x.Sno }).FirstOrDefault();

                        if (!String.IsNullOrEmpty(TdsLedgerLedger.TableKey))
                        {
                            Outstanding osobj1 = new Outstanding();
                            osobj1.Branch = mbranchcode;
                            osobj1.DocBranch = mbranchcode;
                            osobj1.MainType = mModel.MainType;
                            osobj1.SubType = mModel.SubType;
                            osobj1.Type = mModel.Type;
                            osobj1.Prefix = mperiod;
                            osobj1.Srl = mobj.DocNo;
                            osobj1.Sno = DriverLedger.Sno;
                            osobj1.ParentKey = mModel.ParentKey;
                            osobj1.TableKey = DriverLedger.TableKey;
                            osobj1.aMaintype = mModel.MainType;
                            osobj1.aSubType = mModel.SubType;
                            osobj1.aType = mModel.Type;
                            osobj1.aPrefix = mperiod;
                            osobj1.aSrl = mobj.DocNo;
                            osobj1.aSno = TdsLedgerLedger.Sno;
                            //osobj1.Amount = Convert.ToDecimal(item.Amt)+ ((item.ChargesList != null && item.ChargesList.Count > 0) ? item.ChargesList.Where(x => x.AddLess.Trim() == "-").Sum(x => (decimal?)x.Amt) ?? 0 : 0);
                            osobj1.Amount = Convert.ToDecimal(mModel.TDSAmt);
                            osobj1.TableRefKey = TdsLedgerLedger.TableKey;
                            osobj1.AUTHIDS = muserid;
                            osobj1.AUTHORISE = mauthorise;
                            osobj1.BillDate = Convert.ToDateTime(DateTime.Now);
                            osobj1.BillNumber = " ";
                            osobj1.CompCode = mcompcode;
                            osobj1.Broker = 100001;
                            osobj1.Brokerage = Convert.ToDecimal(0.00);
                            osobj1.BrokerAmt = Convert.ToDecimal(0.00);
                            osobj1.BrokerOn = Convert.ToDecimal(0.00);
                            osobj1.ChlnDate = DateTime.Now;
                            osobj1.ChlnNumber = "";
                            osobj1.Code = "000009994";
                            osobj1.CrPeriod = 0;
                            osobj1.CurrName = 0;
                            osobj1.CurrRate = 1;
                            osobj1.DocDate = (DateTime.Now);
                            osobj1.Narr = mModel.Narr;
                            osobj1.OrdDate = DateTime.Now;
                            osobj1.OrdNumber = "";
                            osobj1.ProjCode = "";
                            osobj1.ProjectStage = 0;
                            osobj1.ProjectUnit = 0;
                            osobj1.RefParty = "";
                            osobj1.SalemanAmt = Convert.ToDecimal(0.00);
                            osobj1.SalemanOn = Convert.ToDecimal(0.00);
                            osobj1.SalemanPer = Convert.ToDecimal(0.00);
                            osobj1.Salesman = 100001;
                            osobj1.TDSAmt = 0;
                            osobj1.ENTEREDBY = muserid;
                            osobj1.LASTUPDATEDATE = DateTime.Now;
                            //osobj1.CurrAmount = Convert.ToDecimal(item.Amt) + ((item.ChargesList != null && item.ChargesList.Count > 0) ? item.ChargesList.Where(x => x.AddLess.Trim() == "-").Sum(x => (decimal?)x.Amt) ?? 0 : 0);
                            osobj1.CurrAmount = Convert.ToDecimal(mModel.TDSAmt);
                            osobj1.ValueDate = DateTime.Now;
                            osobj1.LocationCode = 100001;

                            ctxTFAT.Outstanding.Add(osobj1);
                            // second effect
                            Outstanding osobj2 = new Outstanding();
                            osobj2.Branch = mbranchcode;
                            osobj2.DocBranch = mbranchcode;
                            osobj2.aType = mModel.Type;
                            osobj2.aPrefix = mperiod;
                            osobj2.aSrl = mobj.DocNo;
                            osobj2.aSno = DriverLedger.Sno;
                            osobj2.aMaintype = mModel.MainType;
                            osobj2.aSubType = mModel.SubType;
                            osobj2.ParentKey = mModel.ParentKey;
                            osobj2.TableRefKey = DriverLedger.TableKey;
                            osobj2.Type = mModel.Type;
                            osobj2.Prefix = mperiod;
                            osobj2.MainType = mModel.MainType;
                            osobj2.SubType = mModel.SubType;
                            osobj2.Srl = mobj.DocNo;
                            osobj2.Sno = TdsLedgerLedger.Sno;
                            osobj2.TableKey = TdsLedgerLedger.TableKey;
                            //osobj2.Amount = Convert.ToDecimal(item.Amt) + ((item.ChargesList != null && item.ChargesList.Count > 0) ? item.ChargesList.Where(x => x.AddLess.Trim() == "-").Sum(x => (decimal?)x.Amt) ?? 0 : 0);
                            osobj2.Amount = Convert.ToDecimal(mModel.TDSAmt);
                            osobj2.AUTHIDS = muserid;
                            osobj2.AUTHORISE = mauthorise;
                            osobj2.BillDate = Convert.ToDateTime(DateTime.Now);
                            osobj2.BillNumber = " ";
                            osobj2.CompCode = mcompcode;
                            osobj2.Broker = 100001;
                            osobj2.Brokerage = Convert.ToDecimal(0.00);
                            osobj2.BrokerAmt = Convert.ToDecimal(0.00);
                            osobj2.BrokerOn = Convert.ToDecimal(0.00);
                            osobj2.ChlnDate = DateTime.Now;
                            osobj2.ChlnNumber = "";
                            osobj2.Code = "000009994";
                            osobj2.CrPeriod = 0;
                            osobj2.CurrName = 0;
                            osobj2.CurrRate = 1;
                            osobj2.DocDate = (DateTime.Now);
                            osobj2.Narr = mModel.Narr;
                            osobj2.OrdDate = DateTime.Now;
                            osobj2.OrdNumber = "";
                            osobj2.ProjCode = "";
                            osobj2.ProjectStage = 0;
                            osobj2.ProjectUnit = 0;
                            osobj2.RefParty = "";
                            osobj2.SalemanAmt = Convert.ToDecimal(0.00);
                            osobj2.SalemanOn = Convert.ToDecimal(0.00);
                            osobj2.SalemanPer = Convert.ToDecimal(0.00);
                            osobj2.Salesman = 100001;
                            osobj2.TDSAmt = 0;
                            osobj2.ENTEREDBY = muserid;
                            osobj2.LASTUPDATEDATE = DateTime.Now;
                            //osobj2.CurrAmount = Convert.ToDecimal(item.Amt) + ((item.ChargesList != null && item.ChargesList.Count > 0) ? item.ChargesList.Where(x => x.AddLess.Trim() == "-").Sum(x => (decimal?)x.Amt) ?? 0 : 0);
                            osobj2.CurrAmount = Convert.ToDecimal(mModel.TDSAmt);
                            osobj2.ValueDate = DateTime.Now;
                            osobj2.LocationCode = 100001;

                            ctxTFAT.Outstanding.Add(osobj2);
                        }
                    }

                    #endregion

                    ctxTFAT.SaveChanges();

                    transaction.Commit();
                    transaction.Dispose();
                    if (mModel.VehicleFlag == true)
                    {
                        UpdateAuditTrail(mbranchcode, mModel.Mode, mModel.Header, mobj.ParentKey, ConvertDDMMYYTOYYMMDD(mModel.TripSheetDate.ToString()), mModel.NetAmt, mobj.Driver, "Save Trip Sheet", "VM");
                    }
                    else
                    {
                        UpdateAuditTrail(mbranchcode, mModel.Mode, mModel.Header, mobj.ParentKey, ConvertDDMMYYTOYYMMDD(mModel.TripSheetDate.ToString()), mModel.NetAmt, mobj.Driver, "Save Trip Sheet", "DM");
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
            return Json(new { NewSrl = NewSrl, Status = "Success", id = "StateMaster" }, JsonRequestBehavior.AllowGet);
        }

        public void DeUpdate(TripSheetVM Model)
        {
            var tripFmLists = ctxTFAT.TripFmList.Where(x => x.DocNo == Model.Document && x.Prefix == mperiod).ToList();
            var tripExpensesMasters = ctxTFAT.TripExpensesMaster.Where(x => x.DocNo == Model.Document && x.Prefix == mperiod).ToList();
            var mLedgerList = ctxTFAT.Ledger.Where(x => x.ParentKey == Model.ParentKey).ToList();
            var mLedgerList1 = ctxTFAT.Ledger.Where(x => x.ParentKey == Model.TableKey).ToList();
            var mRelatedDataList = ctxTFAT.RelateData.Where(x => x.ParentKey == Model.TableKey).ToList();
            var mRelLRList = ctxTFAT.RelLr.Where(x => x.ParentKey == Model.TableKey).ToList();
            var mRelFmList = ctxTFAT.RelFm.Where(x => x.ParentKey == Model.TableKey).ToList();

            var mOutstanding = ctxTFAT.Outstanding.Where(x => x.ParentKey == Model.ParentKey).ToList();
            ctxTFAT.Outstanding.RemoveRange(mOutstanding);

            foreach (var item in tripExpensesMasters)
            {
                var RelFm = ctxTFAT.RelFm.Where(x => (x.Branch + x.TableKey).ToString().Trim() == item.Branch + item.TableKey).Select(x => x).ToList();
                var RelLR = ctxTFAT.RelLr.Where(x => (x.Branch + x.TableKey).ToString().Trim() == item.Branch + item.TableKey).Select(x => x).ToList();

                ctxTFAT.RelFm.RemoveRange(RelFm);
                ctxTFAT.RelLr.RemoveRange(RelLR);
            }

            ctxTFAT.TripFmList.RemoveRange(tripFmLists);
            ctxTFAT.TripExpensesMaster.RemoveRange(tripExpensesMasters);
            ctxTFAT.Ledger.RemoveRange(mLedgerList);
            ctxTFAT.Ledger.RemoveRange(mLedgerList1);
            ctxTFAT.RelateData.RemoveRange(mRelatedDataList);
            ctxTFAT.RelLr.RemoveRange(mRelLRList);
            ctxTFAT.RelFm.RemoveRange(mRelFmList);
            ctxTFAT.SaveChanges();
        }

        public string DeleteStateMaster(TripSheetVM Model)
        {
            var mobj1 = ctxTFAT.TripSheetMaster.Where(x => x.DocNo == Model.Document && x.Prefix == mperiod).FirstOrDefault();
            Model.TableKey = mobj1.TableKey;
            Model.ParentKey = mobj1.ParentKey;
            var tripFmLists = ctxTFAT.TripFmList.Where(x => x.DocNo == Model.Document && x.Prefix == mperiod).ToList();
            var tripExpensesMasters = ctxTFAT.TripExpensesMaster.Where(x => x.DocNo == Model.Document && x.Prefix == mperiod).ToList();
            var lorryReceiptExpenses = ctxTFAT.LR_FM_Expenses.Where(x => x.Code == Model.Document && x.ParentKey == Model.ParentKey).ToList();
            var mLedgerList = ctxTFAT.Ledger.Where(x => x.ParentKey == Model.ParentKey).ToList();

            var mOutstanding = ctxTFAT.Outstanding.Where(x => x.ParentKey == Model.ParentKey).ToList();
            ctxTFAT.Outstanding.RemoveRange(mOutstanding);
            var mRelatedDataList = ctxTFAT.RelateData.Where(x => x.ParentKey == Model.TableKey).ToList();
            ctxTFAT.RelateData.RemoveRange(mRelatedDataList);

            foreach (var item in tripExpensesMasters)
            {
                var RelFm = ctxTFAT.RelFm.Where(x => (x.Branch + x.TableKey).ToString().Trim() == item.Branch + item.TableKey).Select(x => x).ToList();
                var RelLR = ctxTFAT.RelLr.Where(x => (x.Branch + x.TableKey).ToString().Trim() == item.Branch + item.TableKey).Select(x => x).ToList();

                ctxTFAT.RelFm.RemoveRange(RelFm);
                ctxTFAT.RelLr.RemoveRange(RelLR);
            }

            ctxTFAT.TripSheetMaster.Remove(mobj1);
            ctxTFAT.TripFmList.RemoveRange(tripFmLists);
            ctxTFAT.TripExpensesMaster.RemoveRange(tripExpensesMasters);
            ctxTFAT.LR_FM_Expenses.RemoveRange(lorryReceiptExpenses);
            ctxTFAT.Ledger.RemoveRange(mLedgerList);
            ctxTFAT.SaveChanges();
            return "Success";
        }

        #endregion

        #region Print 

        public ActionResult PrintSingleDocumentCrystal(GridOption Model)
        {
            var PDFName = Model.Document;
            //var relativePath = "~/Reports/" + Model.Format + ".rpt";
            if (Model.Format == null)
            {
                return Json(new { Status = "filenotfound" }, JsonRequestBehavior.AllowGet);
            }

            if (FileExists("~/Reports/" + Model.Format.Trim() + ".rpt") == false)
            {
                return Json(new { Status = "filenotfound" }, JsonRequestBehavior.AllowGet);
            }
            #region Remove Comment
            var absolutePath = HttpContext.Server.MapPath("~/Reports/" + Model.Format.Trim() + ".rpt");
            if (System.IO.File.Exists(absolutePath) == false)
            {
                return Json(new { Status = "filenotfound" }, JsonRequestBehavior.AllowGet);
            }
            #endregion

            //string mParentKey = Model.Document.Substring(6, Model.Document.Length - 6);
            string mParentKey = Model.Document + mperiod;

            DataTable dtreport = new DataTable();
            SqlConnection tfat_conx = new SqlConnection(GetConnectionString());
            SqlCommand cmd = new SqlCommand("SPDoc_" + Model.StoreProcedure, tfat_conx); //name of the storedprocedure
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@mParentKey", mParentKey);
            cmd.Parameters.AddWithValue("@mBranch", mbranchcode);
            SqlDataAdapter adp = new SqlDataAdapter(cmd);
            adp.Fill(dtreport);

            ReportDocument rd = new ReportDocument();
            rd.Load(Path.Combine(Server.MapPath("~/Reports"), Model.Format.Trim() + ".rpt"));
            rd.SetDataSource(dtreport);

            try
            {
                Stream mstream = rd.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
                mstream.Seek(0, SeekOrigin.Begin);
                rd.Close();
                rd.Dispose();
                if (String.IsNullOrEmpty(PDFName))
                {
                    return File(mstream, "application/PDF");
                }
                else
                {
                    return File(mstream, "application/PDF", PDFName + ".pdf");
                }
                //return File(mstream, "application/PDF");
            }
            catch
            {
                rd.Close();
                rd.Dispose();
                throw;
            }
            finally
            {
                rd.Close();
                rd.Dispose();
            }
        }

        public ActionResult SendMultiReport(GridOption Model)
        {
            var PDFName = Model.Document;
            if (Model.Format == null)
            {
                return null;
            }

            string mParentKey = "";
            Document document = new Document(PageSize.A4, 10, 10, 10, 10);
            var ms = new MemoryStream();
            {
                PdfCopy pdf = new PdfCopy(document, ms);
                document.Open();
                pdf.Open();

                List<string> mformats = Model.Format.Split(',').ToList();
                foreach (var mformat in mformats)
                {
                    //mParentKey = Model.Document.Substring(6, Model.Document.Length - 6);
                    mParentKey = Model.Document + mperiod;
                    Model.StoreProcedure = ctxTFAT.DocFormats.Where(x => x.FormatCode.Trim() == mformat.Trim()).Select(x => x.StoredProc).FirstOrDefault();
                    DataTable dtreport = new DataTable();
                    SqlConnection tfat_conx = new SqlConnection(GetConnectionString());
                    SqlCommand cmd = new SqlCommand("SPDoc_" + Model.StoreProcedure, tfat_conx); //name of the storedprocedure
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@mParentKey", mParentKey);
                    cmd.Parameters.AddWithValue("@mBranch", mbranchcode);
                    SqlDataAdapter adp = new SqlDataAdapter(cmd);
                    adp.Fill(dtreport);

                    ReportDocument rd = new ReportDocument();
                    rd.Load(Path.Combine(Server.MapPath("~/Reports"), mformat.Trim() + ".rpt"));
                    rd.SetDataSource(dtreport);
                    try
                    {
                        Stream mstream = rd.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
                        mstream.Seek(0, SeekOrigin.Begin);

                        Warning[] warnings;
                        string[] streamids;
                        string mimeType;
                        string encoding;
                        string extension;
                        MemoryStream memory1 = new MemoryStream();
                        mstream.CopyTo(memory1);
                        byte[] bytes = memory1.ToArray();
                        MemoryStream memoryStream = new MemoryStream(bytes);
                        PdfReader imageDocumentReader = new PdfReader(memoryStream.ToArray());
                        int ab = imageDocumentReader.NumberOfPages;
                        for (int a = 1; a <= ab; a++)
                        {
                            var page = pdf.GetImportedPage(imageDocumentReader, a);
                            pdf.AddPage(page);
                        }
                        imageDocumentReader.Close();
                    }
                    catch
                    {
                        rd.Close();
                        rd.Dispose();
                        throw;
                    }
                    finally
                    {
                        rd.Close();
                        rd.Dispose();
                    }
                }
            }
            document.Close();

            if (String.IsNullOrEmpty(PDFName))
            {
                return File(ms.ToArray(), "application/PDF");
            }
            else
            {
                return File(ms.ToArray(), "application/PDF", PDFName + ".pdf");
            }
            //return File(ms.ToArray(), "application/PDF");

        }

        #endregion

        #region LR And FM Cost Center Handle All Functions

        public List<TripSheetVM> GetLRDetailList(string TableKey)
        {
            List<TripSheetVM> objledgerdetail2 = new List<TripSheetVM>();
            var LRDetailList = ctxTFAT.RelLr.Where(x => (x.Branch + x.TableKey).ToString().Trim() == TableKey).Select(x => x).ToList();
            foreach (var a in LRDetailList)
            {
                objledgerdetail2.Add(new TripSheetVM()
                {
                    LRNumber = a.LrNo,
                    LRAmt = a.LrAmt.Value,
                    tempId = a.SrNo,
                    ConsignmentKey = a.LRRefTablekey,
                });

            }
            return objledgerdetail2;
        }

        public List<TripSheetVM> GetFMDetailList(string TableKey)
        {
            List<TripSheetVM> objledgerdetail2 = new List<TripSheetVM>();
            var LRDetailList = ctxTFAT.RelFm.Where(x => (x.Branch + x.TableKey) == TableKey).Select(x => x).ToList();
            foreach (var a in LRDetailList)
            {
                objledgerdetail2.Add(new TripSheetVM()
                {
                    FMNumber = a.FMNo,
                    FMAmt = a.FmAmt,
                    tempId = a.SrNo,
                    FreightMemoKey = a.FMRefTablekey,
                });


            }
            return objledgerdetail2;
        }

        #region ADD LR details

        public ActionResult AddLRDetails(TripSheetVM Model)
        {
            List<TripSheetVM> lrdetaillist = new List<TripSheetVM>();
            var mlrmaster = ctxTFAT.LRMaster.Where(x => x.TableKey.ToString() == Model.ConsignmentKey).Select(x => x).FirstOrDefault();

            if (mlrmaster == null)
            {
                Model.Status = "ValidError";
                Model.Message = "Consignment Not Found..";
                return Json(Model, JsonRequestBehavior.AllowGet);
            }
            if (Model.SessionFlag == "Add")
            {
                var mRellR = (from r in ctxTFAT.RelLr
                              join rd in ctxTFAT.RelateData on r.TableKey equals rd.TableKey
                              where r.LRRefTablekey == Model.ConsignmentKey && rd.Code == Model.Code
                              select r.TableKey).FirstOrDefault();
                if (mRellR != null)
                {
                    if (Model.NoDuplExpLRFM == true)
                    {
                        Model.Status = "ValidError";
                        Model.Message = /*"LR Already Save Cant Save.."*/"Found Same Expenses on same Document No Cant Save";
                        return Json(Model, JsonRequestBehavior.AllowGet);
                    }
                }
                if (Model.LRDetailList != null)
                {
                    lrdetaillist = Model.LRDetailList;
                }
                if (lrdetaillist.Count() > 0 && (lrdetaillist.Where(x => x.ConsignmentKey == Model.ConsignmentKey).FirstOrDefault() != null))
                {
                    Model.Status = "ValidError";
                    Model.Message = "Consignment Already in List Cant Save..";
                    return Json(Model, JsonRequestBehavior.AllowGet);
                }
                if (String.IsNullOrEmpty(Model.ConsignmentKey))
                {
                    Model.Status = "ConfirmError";
                    Model.Message = "Please Enter The Consignment No..";
                    return Json(Model, JsonRequestBehavior.AllowGet);
                }
                lrdetaillist.Add(new TripSheetVM()
                {
                    ConsignmentKey = Model.ConsignmentKey,
                    LRNumber = Model.LRNumber,
                    LRAmt = Model.LRAmt,
                    tempId = lrdetaillist.Count + 1,
                });
            }
            else
            {
                var mRellR = (from r in ctxTFAT.RelLr
                              join rd in ctxTFAT.RelateData on r.TableKey equals rd.TableKey
                              where r.LRRefTablekey == Model.ConsignmentKey && rd.Code == Model.Code && rd.ParentKey != Model.ParentKey
                              select r.TableKey).FirstOrDefault();
                if (mRellR != null)
                {
                    if (Model.NoDuplExpLRFM == true)
                    {
                        Model.Status = "ValidError";
                        Model.Message = /*"LR Already Save Cant Save.."*/"Found Same Expenses on same Document No Cant Save";
                        return Json(Model, JsonRequestBehavior.AllowGet);
                    }
                }
                if (Model.LRDetailList != null)
                {
                    lrdetaillist = Model.LRDetailList;
                }
                foreach (var item in lrdetaillist.Where(x => x.tempId == Model.tempId))
                {
                    item.ConsignmentKey = Model.ConsignmentKey;
                    item.LRNumber = Model.LRNumber;
                    item.LRAmt = Model.LRAmt;
                    item.tempId = Model.tempId;
                }
            }
            var html = "";
            if (Model.ExpensesAcCurrent)
            {
                html = ViewHelper.RenderPartialView(this, "LRDetailsD", new TripSheetVM() { LRDetailList = lrdetaillist });
            }
            else
            {
                html = ViewHelper.RenderPartialView(this, "LRDetails", new TripSheetVM() { LRDetailList = lrdetaillist });
            }
            return Json(new { LRDetailList = lrdetaillist, Html = html, Amt = lrdetaillist.Sum(x => x.LRAmt) }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult DeleteLRDetails(TripSheetVM Model)
        {
            var result2 = Model.LRDetailList.Where(x => x.tempId != Model.tempId).Select(x => x).ToList();
            var html = "";
            if (Model.ExpensesAcCurrent)
            {
                html = ViewHelper.RenderPartialView(this, "LRDetailsD", new TripSheetVM() { LRDetailList = result2 });
            }
            else
            {
                html = ViewHelper.RenderPartialView(this, "LRDetails", new TripSheetVM() { LRDetailList = result2 });
            }
            return Json(new { LRDetailList = result2, Html = html, Amt = result2.Sum(x => x.LRAmt) }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult GetLRDetails(TripSheetVM Model)
        {
            if (Model.LRDetailList != null && Model.LRDetailList.Count() > 0)
            {
                foreach (var a in Model.LRDetailList.Where(x => x.tempId == Model.tempId))
                {
                    Model.LRNumber = a.LRNumber;
                    Model.LRAmt = a.LRAmt;
                    Model.tempId = a.tempId;
                    Model.ConsignmentKey = a.ConsignmentKey;
                }
            }
            var html = "";
            if (Model.ExpensesAcCurrent)
            {
                html = ViewHelper.RenderPartialView(this, "LRDetailsD", Model);
            }
            else
            {
                html = ViewHelper.RenderPartialView(this, "LRDetails", Model);
            }
            return Json(new { ConsignmentKey = Model.ConsignmentKey, Html = html }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult GetLrMasterDetails(TripSheetVM Model)
        {
            var mlrmaster = ctxTFAT.LRMaster.Where(x => x.TableKey.ToString() == Model.LRNumber).Select(x => x).FirstOrDefault();

            if (mlrmaster == null)
            {
                Model.Status = "ValidError";
                Model.Message = "Consignment Not Found..";
                return Json(Model, JsonRequestBehavior.AllowGet);
            }
            if (mlrmaster != null)
            {
                Model.ConsignmentKey = mlrmaster.TableKey.ToString();
                Model.LRNumber = mlrmaster.LrNo.ToString();
                Model.DocDate = mlrmaster.BookDate;
                Model.Qty = mlrmaster.TotQty;
                Model.ActWt = mlrmaster.ActWt;
                Model.ChgWt = mlrmaster.ChgWt;
                Model.LRFrom = ctxTFAT.TfatBranch.Where(x => x.Code == mlrmaster.Source).Select(x => x.Name).FirstOrDefault();
                Model.LRTo = ctxTFAT.TfatBranch.Where(x => x.Code == mlrmaster.Dest).Select(x => x.Name).FirstOrDefault();
                Model.LRConginer = ctxTFAT.Consigner.Where(x => x.Code == mlrmaster.SendCode).Select(x => x.Name).FirstOrDefault();
                Model.LRConsignee = ctxTFAT.Consigner.Where(x => x.Code == mlrmaster.RecCode).Select(x => x.Name).FirstOrDefault();
                var Setup = ctxTFAT.CreditPurchaseSetup.FirstOrDefault();
                if (Setup != null)
                {
                    if (Setup.ShowConsignmentExp)
                    {
                        var result = (from lrExp in ctxTFAT.RelLr
                                      where lrExp.LRRefTablekey == mlrmaster.TableKey
                                      join Relateda in ctxTFAT.RelateData.Where(x => x.AmtType == true)
                                      on lrExp.TableKey equals Relateda.TableKey
                                      select new TripSheetVM()
                                      {
                                          Amt = lrExp.LrAmt.Value,
                                          AccountName = ctxTFAT.Master.Where(x => x.Code == Relateda.Code).Select(x => x.Name).FirstOrDefault(),
                                          DocDate = Relateda.DocDate.Value,
                                          ENTEREDBY = Relateda.ENTEREDBY,
                                      }).OrderBy(x => x.DocDate).ToList();
                        Model.ConsignmentExplist = result;
                    }
                }
            }
            else
            {
                Model.ConsignmentExplist = new List<TripSheetVM>();
            }
            var html = ViewHelper.RenderPartialView(this, "LRMasterDetails", Model);
            return Json(new { Status = "Success", Html = html }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult FetchDocumentList(TripSheetVM Model)
        {
            List<TripSheetVM> ValueList = new List<TripSheetVM>();

            var mlrmaster = ctxTFAT.LRMaster.Where(x => x.LrNo.ToString() == Model.LRNumber).Select(x => x).ToList();
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
                TripSheetVM otherTransact = new TripSheetVM();
                otherTransact.ConsignmentKey = item.TableKey.ToString();
                otherTransact.LRNumber = item.LrNo.ToString();
                otherTransact.DocDate = item.BookDate;
                otherTransact.Qty = item.TotQty;
                otherTransact.ActWt = item.ActWt;
                otherTransact.ChgWt = item.ChgWt;
                otherTransact.LRFrom = ctxTFAT.TfatBranch.Where(x => x.Code == item.Source).Select(x => x.Name).FirstOrDefault();
                otherTransact.LRTo = ctxTFAT.TfatBranch.Where(x => x.Code == item.Dest).Select(x => x.Name).FirstOrDefault();
                otherTransact.LRConginer = ctxTFAT.Consigner.Where(x => x.Code == item.SendCode).Select(x => x.Name).FirstOrDefault();
                otherTransact.LRConsignee = ctxTFAT.Consigner.Where(x => x.Code == item.RecCode).Select(x => x.Name).FirstOrDefault();
                ValueList.Add(otherTransact);
            }
            Model.ValueList = ValueList;
            var html = ViewHelper.RenderPartialView(this, "ConsignmentList", Model);
            return Json(new { Status = "Success", Html = html }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult ConfirmLRSave(TripSheetVM Model)
        {
            if (Model.SessionFlag == "Add")
            {
                var Setup = ctxTFAT.TripSheetSetup.FirstOrDefault();
                if (Setup != null)
                {
                    if (Setup.RestrictLrDateExp)
                    {
                        var Days = String.IsNullOrEmpty(Setup.RestrictLrExpDays) == true ? 0 : Convert.ToInt32(Setup.RestrictLrExpDays);
                        var DocumentDate = ConvertDDMMYYTOYYMMDD(Model.TripSheetDate);
                        DocumentDate = DocumentDate.AddDays(-Days);

                        var ConsignemtDate = ctxTFAT.LRMaster.Where(x => x.TableKey == Model.ConsignmentKey).Select(x => x.BookDate).FirstOrDefault();
                        if (!(DocumentDate <= ConsignemtDate))
                        {
                            Model.Status = "ValidError";
                            Model.Message = "Consignemt Date Always Greater Than '" + DocumentDate.ToShortDateString() + "' Only...";
                        }

                    }
                }

                var mRellR = (from r in ctxTFAT.RelLr
                              join rd in ctxTFAT.RelateData on r.TableKey equals rd.TableKey
                              where r.LRRefTablekey == Model.ConsignmentKey && rd.Code == Model.Code
                              select r.TableKey).FirstOrDefault();

                if (mRellR != null)
                {
                    Model.Status = "ConfirmError";
                    Model.Message = "Document has already same Charges Do you want to Continue..";
                }
            }
            else
            {
                var mRellR = (from r in ctxTFAT.RelLr
                              join rd in ctxTFAT.RelateData on r.TableKey equals rd.TableKey
                              where r.LRRefTablekey == Model.ConsignmentKey && rd.Code == Model.Code && rd.ParentKey != Model.ParentKey
                              select r.TableKey).FirstOrDefault();

                if (mRellR != null)
                {

                    Model.Status = "ConfirmError";
                    Model.Message = "Document has already same Charges Do you want to Continue..";
                    return Json(new { Status = Model.Status, Message = Model.Message }, JsonRequestBehavior.AllowGet);

                }

            }
            return Json(new { Status = Model.Status, Message = Model.Message }, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region ADD FM details

        public ActionResult AddFMDetails(TripSheetVM Model)
        {
            List<TripSheetVM> FMdetaillist = new List<TripSheetVM>();
            if (Model.SessionFlag == "Add")
            {

                var mRellR = (from r in ctxTFAT.RelFm
                              join rd in ctxTFAT.RelateData on r.TableKey equals rd.TableKey
                              where r.FMRefTablekey == Model.FreightMemoKey && rd.Code == Model.Code
                              select r.TableKey).FirstOrDefault();
                if (mRellR != null)
                {
                    if (Model.NoDuplExpLRFM == true)
                    {
                        Model.Status = "ValidError";
                        Model.Message = /*"FM Already Save Cant Save.."*/"Found Same Expenses on same Document No Cant Save";
                        return Json(Model, JsonRequestBehavior.AllowGet);
                    }
                }
                if (Model.FMDetailList != null)
                {
                    FMdetaillist = Model.FMDetailList;
                }
                if (FMdetaillist.Count() > 0 && (FMdetaillist.Where(x => x.FreightMemoKey == Model.FreightMemoKey).FirstOrDefault() != null))
                {
                    Model.Status = "ValidError";
                    Model.Message = "Freight Memo Already in List Cant Save..";
                    return Json(Model, JsonRequestBehavior.AllowGet);
                }
                if (String.IsNullOrEmpty(Model.FMNumber))
                {
                    Model.Status = "ConfirmError";
                    Model.Message = "Please Enter The Freight Memo No..";
                    return Json(Model, JsonRequestBehavior.AllowGet);
                }
                FMdetaillist.Add(new TripSheetVM()
                {
                    FMNumber = Model.FMNumber,
                    FMAmt = Model.FMAmt,
                    tempId = FMdetaillist.Count + 1,
                    FreightMemoKey = Model.FreightMemoKey,
                });
            }
            else
            {
                var mRellR = (from r in ctxTFAT.RelFm
                              join rd in ctxTFAT.RelateData on r.TableKey equals rd.TableKey
                              where r.FMRefTablekey == Model.FreightMemoKey && rd.Code == Model.Code && rd.ParentKey != Model.ParentKey
                              select r.TableKey).FirstOrDefault();
                if (mRellR != null)
                {
                    if (Model.NoDuplExpLRFM == true)
                    {
                        Model.Status = "ValidError";
                        Model.Message = /*"FM Already Save Cant Save.."*/"Found Same Expenses on same Document No Cant Save";
                        return Json(Model, JsonRequestBehavior.AllowGet);
                    }
                }
                if (Model.FMDetailList != null)
                {
                    FMdetaillist = Model.FMDetailList;
                }
                foreach (var item in FMdetaillist.Where(x => x.tempId == Model.tempId))
                {

                    item.FMNumber = Model.FMNumber;
                    item.FMAmt = Model.FMAmt;
                    item.tempId = Model.tempId;
                    item.FreightMemoKey = Model.FreightMemoKey;
                }
            }
            var html = "";
            if (Model.ExpensesAcCurrent)
            {
                html = ViewHelper.RenderPartialView(this, "FMDetailsD", new TripSheetVM() { FMDetailList = FMdetaillist });
            }
            else
            {
                html = ViewHelper.RenderPartialView(this, "FMDetails", new TripSheetVM() { FMDetailList = FMdetaillist });
            }

            return Json(new { FMDetailList = FMdetaillist, Html = html, Amt = FMdetaillist.Sum(x => x.FMAmt) }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult DeleteFMDetails(TripSheetVM Model)
        {
            var result2 = Model.FMDetailList.Where(x => x.tempId != Model.tempId).Select(x => x).ToList();
            var html = "";
            if (Model.ExpensesAcCurrent)
            {
                html = ViewHelper.RenderPartialView(this, "FMDetailsD", new TripSheetVM() { FMDetailList = result2 });
            }
            else
            {
                html = ViewHelper.RenderPartialView(this, "FMDetails", new TripSheetVM() { FMDetailList = result2 });
            }
            return Json(new { FMDetailList = result2, Html = html, Amt = result2.Sum(x => x.FMAmt) }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult GetFMDetails(TripSheetVM Model)
        {
            if (Model.FMDetailList != null && Model.FMDetailList.Count() > 0)
            {
                foreach (var a in Model.FMDetailList.Where(x => x.tempId == Model.tempId))
                {
                    Model.FMNumber = a.FMNumber;
                    Model.FMAmt = a.FMAmt;
                    Model.tempId = a.tempId;
                    Model.FreightMemoKey = a.FreightMemoKey;
                }
            }
            var html = "";
            if (Model.ExpensesAcCurrent)
            {
                html = ViewHelper.RenderPartialView(this, "FMDetailsD", Model);
            }
            else
            {
                html = ViewHelper.RenderPartialView(this, "FMDetails", Model);
            }
            return Json(new { FreightMemoKey = Model.FreightMemoKey, Html = html }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult GetFMMasterDetails(TripSheetVM Model)
        {
            var mlrmaster = ctxTFAT.FMMaster.Where(x => x.TableKey.ToString() == Model.FMNumber).Select(x => x).FirstOrDefault();

            if (mlrmaster == null)
            {
                Model.Status = "ValidError";
                Model.Message = "Freight Memo Not Found..";
                return Json(Model, JsonRequestBehavior.AllowGet);
            }

            if (mlrmaster != null)
            {
                Model.FMNumber = mlrmaster.FmNo.ToString();
                Model.FreightMemoKey = mlrmaster.TableKey.ToString();
                Model.DocDate = mlrmaster.Date;
                Model.Name = ctxTFAT.VehicleCategory.Where(x => x.Code == mlrmaster.VehicleCategory).Select(x => x.VehicleCategory1).FirstOrDefault();
                Model.LRFrom = ctxTFAT.TfatBranch.Where(x => x.Code == mlrmaster.FromBranch).Select(x => x.Name).FirstOrDefault();
                Model.LRTo = ctxTFAT.TfatBranch.Where(x => x.Code == mlrmaster.ToBranch).Select(x => x.Name).FirstOrDefault();
                Model.LRConginer = ctxTFAT.DriverMaster.Where(x => x.Code == mlrmaster.Driver).Select(x => x.Name).FirstOrDefault();
                Model.LRConsignee = mlrmaster.VehicleStatus == "100001" ? ctxTFAT.HireVehicleMaster.Where(x => x.Code == mlrmaster.TruckNo).Select(x => x.TruckNo).FirstOrDefault() : ctxTFAT.VehicleMaster.Where(x => x.Code == mlrmaster.TruckNo).Select(x => x.TruckNo).FirstOrDefault();
            }


            var html = ViewHelper.RenderPartialView(this, "FMMasterDetails", Model);
            return Json(new { Html = html }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult FetchFreightMemoDocumentList(TripSheetVM Model)
        {
            List<TripSheetVM> ValueList = new List<TripSheetVM>();

            var mlrmaster = ctxTFAT.FMMaster.Where(x => x.FmNo.ToString() == Model.FMNumber).Select(x => x).ToList();
            if (mlrmaster == null)
            {
                Model.Status = "ValidError";
                Model.Message = "Freight Memo Not Found..";
                return Json(Model, JsonRequestBehavior.AllowGet);
            }
            if (mlrmaster.Count() == 0)
            {
                Model.Status = "ValidError";
                Model.Message = "Freight Memo Not Found..";
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
                TripSheetVM otherTransact = new TripSheetVM();
                otherTransact.FMNumber = item.FmNo.ToString();
                otherTransact.FreightMemoKey = item.TableKey.ToString();
                otherTransact.DocDate = item.Date;
                otherTransact.Name = ctxTFAT.VehicleCategory.Where(x => x.Code == item.VehicleCategory).Select(x => x.VehicleCategory1).FirstOrDefault();
                otherTransact.LRFrom = ctxTFAT.TfatBranch.Where(x => x.Code == item.FromBranch).Select(x => x.Name).FirstOrDefault();
                otherTransact.LRTo = ctxTFAT.TfatBranch.Where(x => x.Code == item.ToBranch).Select(x => x.Name).FirstOrDefault();
                otherTransact.LRConginer = ctxTFAT.DriverMaster.Where(x => x.Code == item.Driver).Select(x => x.Name).FirstOrDefault();
                otherTransact.LRConsignee = item.VehicleStatus == "100001" ? ctxTFAT.HireVehicleMaster.Where(x => x.Code == item.TruckNo).Select(x => x.TruckNo).FirstOrDefault() : ctxTFAT.VehicleMaster.Where(x => x.Code == item.TruckNo).Select(x => x.TruckNo).FirstOrDefault();
                ValueList.Add(otherTransact);
            }
            Model.ValueList = ValueList;
            var html = ViewHelper.RenderPartialView(this, "FreightMemoList", Model);
            return Json(new { Status = "Success", Html = html }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult ConfirmFMSave(TripSheetVM Model)
        {

            if (Model.SessionFlag == "Add")
            {
                var mRellR = (from r in ctxTFAT.RelFm
                              join rd in ctxTFAT.RelateData on r.TableKey equals rd.TableKey
                              where r.FMRefTablekey == Model.FreightMemoKey && rd.Code == Model.Code
                              select r.TableKey).FirstOrDefault();

                if (mRellR != null)
                {

                    Model.Status = "ConfirmError";
                    Model.Message = "Freight Memo Already Save Do you want to Continue..";


                }
            }
            else
            {
                var mRellR = (from r in ctxTFAT.RelFm
                              join rd in ctxTFAT.RelateData on r.TableKey equals rd.TableKey
                              where r.FMRefTablekey == Model.FreightMemoKey && rd.Code == Model.Code && rd.ParentKey != Model.ParentKey
                              select r.TableKey).FirstOrDefault();

                if (mRellR != null)
                {

                    Model.Status = "ConfirmError";
                    Model.Message = "Freight Memo Already Save Do you want to Continue..";
                    return Json(new { Status = Model.Status, Message = Model.Message }, JsonRequestBehavior.AllowGet);

                }
            }
            return Json(new { Status = Model.Status, Message = Model.Message }, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #endregion


        #region NarrOfBranchWise

        public ActionResult ShowNarrDocument(string FMNO, bool FMORNOT)
        {
            List<FmNarrBranchWise> FMList = new List<FmNarrBranchWise>();
            if (FMORNOT)
            {
                FMList = (from FreightMemo in ctxTFAT.FMMaster
                          where FreightMemo.TableKey.ToString().Trim() == FMNO.ToString().Trim() && String.IsNullOrEmpty(FreightMemo.Remark) == false
                          orderby FreightMemo.FmNo
                          select new FmNarrBranchWise()
                          {
                              FMno = FreightMemo.FmNo.ToString(),
                              Branch = ctxTFAT.TfatBranch.Where(x => x.Code == FreightMemo.Branch).Select(x => x.Name).FirstOrDefault(),
                              Narr = FreightMemo.Remark,
                              Description = "Freight Memo",
                          }).ToList();

                var ArrivalFMList = (from FreightMemo in ctxTFAT.FMROUTETable
                                     where FreightMemo.Parentkey.ToString().Trim() == FMNO.ToString().Trim() && String.IsNullOrEmpty(FreightMemo.ArrivalRemark) == false
                                     orderby FreightMemo.FmNo
                                     select new FmNarrBranchWise()
                                     {
                                         FMno = FreightMemo.FmNo.ToString(),
                                         Branch = ctxTFAT.TfatBranch.Where(x => x.Code == FreightMemo.Parent).Select(x => x.Name).FirstOrDefault(),
                                         Narr = FreightMemo.Narr,
                                         Description = "Arrival",
                                     }).ToList();
                var DispatchFMList = (from FreightMemo in ctxTFAT.FMROUTETable
                                      where FreightMemo.Parentkey.ToString().Trim() == FMNO.ToString().Trim() && String.IsNullOrEmpty(FreightMemo.DispatchRemark) == false
                                      orderby FreightMemo.FmNo
                                      select new FmNarrBranchWise()
                                      {
                                          FMno = FreightMemo.FmNo.ToString(),
                                          Branch = ctxTFAT.TfatBranch.Where(x => x.Code == FreightMemo.Parent).Select(x => x.Name).FirstOrDefault(),
                                          Narr = FreightMemo.Narr,
                                          Description = "Dispatch",
                                      }).ToList();

                var VehicleActivityFMList = (from FreightMemo in ctxTFAT.AlertNoteMaster
                                             where FreightMemo.ParentKey.ToString().Trim() == FMNO.ToString().Trim() && FreightMemo.Type == "FM000" && (FreightMemo.RefType.Contains("FM000") || String.IsNullOrEmpty(FreightMemo.RefType) == true)
                                             orderby FreightMemo.DocNo
                                             select new FmNarrBranchWise()
                                             {
                                                 FMno = FreightMemo.TypeCode.ToString(),
                                                 Branch = ctxTFAT.TfatBranch.Where(x => x.Code == FreightMemo.Branch).Select(x => x.Name).FirstOrDefault(),
                                                 Narr = FreightMemo.Note,
                                                 Description = "AlertNote",
                                             }).ToList();

                FMList.AddRange(ArrivalFMList);
                FMList.AddRange(DispatchFMList);
                FMList.AddRange(VehicleActivityFMList);
            }
            else
            {
                FMList = (from FreightMemo in ctxTFAT.LRMaster
                          where FreightMemo.TableKey.ToString().Trim() == FMNO.ToString().Trim() && String.IsNullOrEmpty(FreightMemo.Narr) == false
                          orderby FreightMemo.LrNo
                          select new FmNarrBranchWise()
                          {
                              FMno = FreightMemo.LrNo.ToString(),
                              Branch = ctxTFAT.TfatBranch.Where(x => x.Code == FreightMemo.Branch).Select(x => x.Name).FirstOrDefault(),
                              Narr = FreightMemo.Narr,
                              Description = "Consignment",
                          }).ToList();
                var VehicleActivityFMList = (from FreightMemo in ctxTFAT.AlertNoteMaster
                                             where FreightMemo.ParentKey.ToString().Trim() == FMNO.ToString().Trim() && FreightMemo.Type == "LR000" && (FreightMemo.RefType.Contains("LR000") || String.IsNullOrEmpty(FreightMemo.RefType) == true)
                                             orderby FreightMemo.DocNo
                                             select new FmNarrBranchWise()
                                             {
                                                 FMno = FreightMemo.TypeCode.ToString(),
                                                 Branch = ctxTFAT.TfatBranch.Where(x => x.Code == FreightMemo.Branch).Select(x => x.Name).FirstOrDefault(),
                                                 Narr = FreightMemo.Note,
                                                 Description = "AlertNote",
                                             }).ToList();
                FMList.AddRange(VehicleActivityFMList);
            }
            List<FmNarrBranchWise> fMMasters = new List<FmNarrBranchWise>();
            if (FMORNOT)
            {
                fMMasters = FMList;
            }
            var html = ViewHelper.RenderPartialView(this, "DocNarrBranchWise", new TripSheetVM() { FMNarrDetailList = fMMasters });

            return Json(new { Html = html }, JsonRequestBehavior.AllowGet);

        }

        #endregion

        #region Schedule Show OF Document

        public ActionResult ShowScheduleDocument(string FMNO)
        {
            List<FMROUTETable> FMROUTETables = ctxTFAT.FMROUTETable.Where(x => x.Parentkey.ToString() == FMNO).OrderBy(x => x.SubRoute).ToList();
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

            var html = ViewHelper.RenderPartialView(this, "DocumentScheduleDetails", new TripSheetVM() { ScheduleDetails = routeDetails, FMNumber = FMNO });

            return Json(new { Html = html }, JsonRequestBehavior.AllowGet);

        }

        #endregion
    }
}